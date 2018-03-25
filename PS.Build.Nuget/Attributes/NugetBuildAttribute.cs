using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NuGet.Frameworks;
using NuGet.Packaging;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;
using PS.Build.Types;
using NugetPackage = PS.Build.Nuget.Types.NugetPackage;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetBuildAttribute : BaseNugetAttribute
    {
        #region Static members

        private static void DumpAssemblyReferences(ILogger logger, NugetPackage package)
        {
            logger.Info("Package assembly references: " + (package.Metadata.PackageAssemblyReferences.Any() ? string.Empty : "None"));
            var regroupedPackageAssemblyReferences = package.Metadata.PackageAssemblyReferences
                                                            .SelectMany(r => r.References
                                                                              .Select(f => new
                                                                              {
                                                                                  r.TargetFramework,
                                                                                  AssemblyName = f
                                                                              }))
                                                            .ToLookup(s => s.AssemblyName, s => s.TargetFramework);
            foreach (var group in regroupedPackageAssemblyReferences)
            {
                using (logger.IndentMessages())
                {
                    logger.Info("Assembly: " + group.Key);
                    foreach (var framework in group)
                    {
                        using (logger.IndentMessages())
                        {
                            logger.Info("Framework: " + framework);
                        }
                    }
                }
            }
        }

        private static void DumpFrameworkReferences(ILogger logger, NugetPackage package)
        {
            logger.Info("Package framework references: " + (package.Metadata.FrameworkReferences.Any() ? string.Empty : "None"));

            foreach (var reference in package.Metadata.FrameworkReferences)
            {
                using (logger.IndentMessages())
                {
                    logger.Info("Assembly: " + reference.AssemblyName);
                    foreach (var framework in reference.SupportedFrameworks)
                    {
                        using (logger.IndentMessages())
                        {
                            logger.Info("Framework: " + framework);
                        }
                    }
                }
            }
        }

        private static void DumpMetadata(ILogger logger, NugetPackage package)
        {
            if (package.Metadata.Copyright != null) logger.Info("Copyright: " + package.Metadata.Copyright);
            if (package.Metadata.Description != null) logger.Info("Description: " + package.Metadata.Description);
            logger.Info("Development dependency: " + package.Metadata.DevelopmentDependency);
            if (package.Metadata.IconUrl != null) logger.Info("Icon url: " + package.Metadata.IconUrl);
            if (package.Metadata.Language != null) logger.Info("Language: " + package.Metadata.Language);
            if (package.Metadata.LicenseUrl != null) logger.Info("License url: " + package.Metadata.LicenseUrl);
            if (package.Metadata.MinClientVersion != null) logger.Info("Minimum client version: " + package.Metadata.MinClientVersionString);
            if (package.Metadata.ProjectUrl != null) logger.Info("Project url: " + package.Metadata.ProjectUrl);
            if (package.Metadata.ReleaseNotes != null) logger.Info("Release notes: " + package.Metadata.ReleaseNotes);
            logger.Info("Require license acceptance: " + package.Metadata.RequireLicenseAcceptance);
            logger.Info("Serviceable: " + package.Metadata.Serviceable);
            if (package.Metadata.Summary != null) logger.Info("Summary: " + package.Metadata.Summary);
            if (package.Metadata.Tags != null) logger.Info("Tags: " + package.Metadata.Tags);
            if (package.Metadata.Title != null) logger.Info("Title: " + package.Metadata.Title);
        }

        private static void DumpPackageDependencies(ILogger logger, NugetPackage package)
        {
            logger.Info("Package dependencies: " + (package.Metadata.DependencyGroups.Any() ? string.Empty : "None"));
            foreach (var dependencyGroup in package.Metadata.DependencyGroups)
            {
                using (logger.IndentMessages())
                {
                    logger.Info("Dependency group: " + dependencyGroup.TargetFramework);
                    foreach (var dependency in dependencyGroup.Packages)
                    {
                        using (logger.IndentMessages())
                        {
                            logger.Info("Dependency: " + dependency);
                        }
                    }
                }
            }
        }

        private static void ProcessPackageDependencies(NugetPackage package)
        {
            if (package.IncludeDependencies.Any())
            {
                var includeLookup = package.IncludeDependencies.ToLookup(d => d.TargetFramework, d => d);
                var excludeLookup = package.ExcludeDependencies.ToLookup(d => d.Framework, d => d.Mask);

                foreach (var group in includeLookup)
                {
                    var bannedPackages = new List<string>();
                    if (excludeLookup.Contains(group.Key))
                    {
                        bannedPackages.AddRange(
                            excludeLookup[group.Key].SelectMany(
                                e => PathExtensions.Match(group.Select(g => g.PackageIdentity.Id), e)));
                    }

                    if (excludeLookup.Contains(NuGetFramework.AnyFramework))
                    {
                        bannedPackages.AddRange(
                            excludeLookup[NuGetFramework.AnyFramework].SelectMany(
                                e => PathExtensions.Match(group.Select(g => g.PackageIdentity.Id), e)));
                    }

                    foreach (var reference in group)
                    {
                        if (bannedPackages.Contains(reference.PackageIdentity.Id)) continue;
                        package.Metadata.AddDependency(reference);
                    }
                }
            }
        }

        #endregion

        private readonly string _targetDirectory;

        #region Constructors

        public NugetBuildAttribute(string targetDirectory = null)
        {
            _targetDirectory = targetDirectory;
        }

        #endregion

        #region Members

        private void PostBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var package = provider.GetVaultPackage(ID);

            logger.Info("Building nuget package");
            using (logger.IndentMessages())
            {
                var targetDirectory = string.IsNullOrWhiteSpace(_targetDirectory)
                    ? provider.GetService<IExplorer>().Directories[BuildDirectory.Target]
                    : _targetDirectory;

                targetDirectory = provider.GetService<IMacroResolver>().Resolve(targetDirectory);

                logger.Info("Target directory: " + targetDirectory);
                logger.Info("ID: " + package.Metadata.Id);
                logger.Info("Version: " + package.Metadata.Version);

                ProcessPackageDependencies(package);

                DumpMetadata(logger, package);
                DumpFrameworkReferences(logger, package);
                DumpAssemblyReferences(logger, package);
                DumpPackageDependencies(logger, package);

                try
                {
                    targetDirectory.EnsureDirectoryExist();

                    var build = new PackageBuilder();
                    build.Populate(package.Metadata);

                    var temporaryDirectory = provider.GetService<IExplorer>().Directories[BuildDirectory.Intermediate];
                    var files = package.EnumerateFiles(logger).ToList();

                    if (files.Any(f => f.Encrypt) && package.X509Certificate == null)
                    {
                        var message = "Some files requires encryption but encrypt certificate was not set";
                        throw new InvalidOperationException(message);
                    }

                    var encryptionSession = new EncryptionSession(temporaryDirectory, package.X509Certificate);

                    foreach (var file in files)
                    {
                        if (file.Encrypt)
                        {
                            var encryptedFilePath = Path.Combine(encryptionSession.EncryptedFilesDirectory,
                                                                 Path.GetFileName(file.Source) ?? string.Empty);

                            var encryptionFile = encryptionSession.EncryptFile(file.Source, encryptedFilePath);
                            build.AddFiles(targetDirectory, encryptedFilePath, file.Destination);

                            encryptionFile.Origin = build.Files.LastOrDefault()?.Path;
                        }
                        else
                        {
                            build.AddFiles(targetDirectory, file.Source, file.Destination);
                        }
                    }

                    if (encryptionSession.Configuration.Files.Any())
                    {
                        build.AddFiles(targetDirectory, encryptionSession.SaveConfiguration(), string.Empty);

                        var buildNugetPackageFolder = provider.GetService<IMacroResolver>().Resolve("{nuget.PS.Build.Nuget}") ?? string.Empty;
                        var decryptorFilePath = Path.Combine(buildNugetPackageFolder, @"tools\decryptor.exe");
                        if (!File.Exists(decryptorFilePath)) throw new FileNotFoundException("Decryptor tool was not found.", decryptorFilePath);
                        build.AddFiles(targetDirectory, decryptorFilePath, string.Empty);

                        if (package.X509CertificateExport)
                        {
                            logger.Debug("Exporting encryption certificate...");
                            var cryptoProvider = package.X509Certificate.PrivateKey as RSACryptoServiceProvider;
                            if (cryptoProvider == null)
                            {
                                var message = "Certificate private key is unavailable. Certificate cannot be exported.";
                                throw new InvalidOperationException(message);
                            }

                            if (!cryptoProvider.CspKeyContainerInfo.Exportable)
                            {
                                var message = "Encryption certificate is not exportable. Certificate cannot be exported.";
                                throw new InvalidOperationException(message);
                            }

                            logger.Debug("Trying to export encryption certificate...");
                            var certificatePath = Path.Combine(targetDirectory, package.Metadata.Id + "." + package.Metadata.Version + ".pfx");

                            var bytes = string.IsNullOrWhiteSpace(package.X509CertificatePassword)
                                ? package.X509Certificate.Export(X509ContentType.Pfx)
                                : package.X509Certificate.Export(X509ContentType.Pfx, package.X509CertificatePassword);

                            File.WriteAllBytes(certificatePath, bytes);
                            logger.Info("Encryption PFX certificate was exported to target directory");
                        }
                    }

                    var finalPath = Path.Combine(targetDirectory, package.Metadata.Id + "." + package.Metadata.Version + ".nupkg");
                    if (File.Exists(finalPath)) File.Delete(finalPath);

                    using (var stream = File.OpenWrite(finalPath))
                    {
                        logger.Debug("Building...");
                        build.Save(stream);
                    }

                    logger.Info("Package successfully created");
                }
                catch (Exception e)
                {
                    logger.Error("Package build failed. Details: " + e.GetBaseException().Message);
                }
            }
        }

        #endregion
    }
}