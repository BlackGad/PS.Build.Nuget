using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Versioning;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes;
using PS.Build.Nuget.Types;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Nuget.Extensions
{
    internal static class ServiceProviderExtensions
    {
        #region Static members

        internal static NugetPackage GetVaultPackage(this IServiceProvider provider, string id)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            var vault = provider.GetService<IDynamicVault>();
            if (vault == null) throw new ArgumentNullException(nameof(vault));
            vault.Query(() => new NugetEnvironment(provider));
            
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            id = id.ToLowerInvariant();
            return vault.Query(id, () => CreateInitialPackage(provider, id));
        }

        private static NugetPackage CreateInitialPackage(IServiceProvider provider, string id)
        {
            var compilation = provider.GetService<Microsoft.CodeAnalysis.CSharp.CSharpCompilation>();
            var explorer = provider.GetService<IExplorer>();
            var logger = provider.GetService<ILogger>();

            logger.Debug("Creating intial '" + id + "' package record");

            var metadata = new ManifestMetadata
            {
                Id = id,
            };

            var versionAttribute = compilation.Assembly.GetAttribute<AssemblyVersionAttribute>();
            if (!string.IsNullOrEmpty(versionAttribute?.Version))
            {
                logger.Debug(" Assembly version attribute is: " + versionAttribute.Version);
                NuGetVersion nugetVersion;
                if (NuGetVersion.TryParse(versionAttribute.Version, out nugetVersion))
                {
                    logger.Debug(" + Nuget package version: " + nugetVersion);
                    metadata.Version = nugetVersion;
                }
                else
                {
                    logger.Debug(" - Could not parse version");
                }
            }
            else logger.Debug(" Assembly version attribute is empty or not defined");

            var companyAttribute = compilation.Assembly.GetAttribute<AssemblyCompanyAttribute>();
            if (!string.IsNullOrEmpty(companyAttribute?.Company))
            {
                logger.Debug(" Assembly company attribute is: " + companyAttribute.Company);
                metadata.Owners = new[] { companyAttribute.Company };
                logger.Debug(" + Nuget package owner: " + companyAttribute.Company);
            }
            else logger.Debug(" Assembly company attribute is empty or not defined");

            var titleAttribute = compilation.Assembly.GetAttribute<AssemblyTitleAttribute>();
            if (!string.IsNullOrEmpty(titleAttribute?.Title))
            {
                logger.Debug(" Assembly title attribute is: " + titleAttribute.Title);
                metadata.Title = titleAttribute.Title;
                logger.Debug(" + Nuget package title: " + titleAttribute.Title);
            }
            else logger.Debug(" Assembly title attribute is empty or not defined");

            var descriptionAttribute = compilation.Assembly.GetAttribute<AssemblyDescriptionAttribute>();
            if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
            {
                logger.Debug(" Assembly description attribute is: " + descriptionAttribute.Description);
                metadata.Description = descriptionAttribute.Description;
                logger.Debug(" + Nuget package description: " + descriptionAttribute.Description);
            }
            else logger.Debug(" Assembly description attribute is empty or not defined");

            var copyrightAttribute = compilation.Assembly.GetAttribute<AssemblyCopyrightAttribute>();
            if (!string.IsNullOrEmpty(copyrightAttribute?.Copyright))
            {
                logger.Debug(" Assembly copyright attribute is: " + copyrightAttribute.Copyright);
                metadata.Copyright = copyrightAttribute.Copyright;
                logger.Debug(" + Nuget package copyright: " + copyrightAttribute.Copyright);
            }
            else logger.Debug(" Assembly copyright attribute is empty or not defined");

            var cultureAttribute = compilation.Assembly.GetAttribute<AssemblyCultureAttribute>();
            if (!string.IsNullOrEmpty(cultureAttribute?.Culture))
            {
                logger.Debug(" Assembly culture attribute is: " + cultureAttribute.Culture);
                metadata.Language = cultureAttribute.Culture;
                logger.Debug(" + Nuget package language: " + cultureAttribute.Culture);
            }
            else logger.Debug(" Assembly culture attribute is empty or not defined");

            var packagesConfig = explorer.Items[BuildItem.None]
                .FirstOrDefault(i => string.Equals(i.Identity,
                                                   "packages.config",
                                                   StringComparison.InvariantCultureIgnoreCase));

            if (packagesConfig != null && File.Exists(packagesConfig.FullPath))
            {
                try
                {
                    logger.Debug(" Reading...");
                    var configReader = new PackagesConfigReader(XDocument.Parse(File.ReadAllText(packagesConfig.FullPath)));
                    var dependencies = configReader.GetPackages(false);
                    foreach (var dependency in dependencies)
                    {
                        if (!dependency.IsUserInstalled)
                        {
                            logger.Debug(
                                $" - Dependency '{dependency.PackageIdentity.Id}' installed automatically as dependecy for another package. Skipping.");
                            continue;
                        }
                        if (dependency.PackageIdentity.Id.StartsWith("PS.Build"))
                        {
                            logger.Debug($" - Dependency '{dependency.PackageIdentity.Id}' id starts from 'PS.Build'. Skipping.");
                            continue;
                        }

                        var versionRange = dependency.PackageIdentity.HasVersion ? dependency.PackageIdentity.Version.ToString() : null;
                        if (dependency.HasAllowedVersions) versionRange = dependency.AllowedVersions.ToString();
                        NugetPackageDependencyAttribute.AddDependency(metadata,
                                                                      dependency.PackageIdentity.Id,
                                                                      versionRange,
                                                                      NuGetFramework.AnyFramework);
                        logger.Debug($" + Nuget dependency '{dependency.PackageIdentity.Id}' added.");
                    }
                }
                catch (Exception e)
                {
                    logger.Debug(" Configuration file packages.config parse error. Details: " + e.GetBaseException().Message);
                }
            }
            else logger.Debug(" Configuration file packages.config not found");

            var result = new NugetPackage(metadata);

            var targetFrameworkVersionString = explorer.Properties[BuildProperty.TargetFrameworkVersion]?.Replace("v", string.Empty);
            Version targetFrameworkVersion;
            if (!Version.TryParse(targetFrameworkVersionString, out targetFrameworkVersion))
            {
                logger.Debug(" Target framework version is invalid");
            }
            else
            {
                var targetOutput = explorer.Properties[BuildProperty.TargetPath];
                var nugetFramework = new NuGetFramework(FrameworkConstants.FrameworkIdentifiers.Net, targetFrameworkVersion);
                result.Files.Add(new NugetPackageFiles(targetOutput, Path.Combine("lib", nugetFramework.GetShortFolderName())));
            }

            return result;
        }

        #endregion
    }
}