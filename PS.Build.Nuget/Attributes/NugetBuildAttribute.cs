using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetBuildAttribute : BaseNugetAttribute
    {
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
            var targetDirectory = string.IsNullOrWhiteSpace(_targetDirectory)
                ? provider.GetService<IExplorer>().Directories[BuildDirectory.Target]
                : _targetDirectory;

            logger.Info("Building nuget package");
            logger.Info("  Target directory: " + targetDirectory);
            logger.Info("  ID: " + package.Metadata.Id);
            logger.Info("  Version: " + package.Metadata.Version);

            if (package.Metadata.Copyright != null) logger.Info("  Copyright: " + package.Metadata.Copyright);
            if (package.Metadata.Description != null) logger.Info("  Description: " + package.Metadata.Description);
            logger.Info("  Development dependency: " + package.Metadata.DevelopmentDependency);
            if (package.Metadata.IconUrl != null) logger.Info("  Icon url: " + package.Metadata.IconUrl);
            if (package.Metadata.Language != null) logger.Info("  Language: " + package.Metadata.Language);
            if (package.Metadata.LicenseUrl != null) logger.Info("  License url: " + package.Metadata.LicenseUrl);
            if (package.Metadata.MinClientVersion != null) logger.Info("  Minimum client version: " + package.Metadata.MinClientVersionString);
            if (package.Metadata.ProjectUrl != null) logger.Info("  Project url: " + package.Metadata.ProjectUrl);
            if (package.Metadata.ReleaseNotes != null) logger.Info("  Release notes: " + package.Metadata.ReleaseNotes);
            logger.Info("  Require license acceptance: " + package.Metadata.RequireLicenseAcceptance);
            logger.Info("  Serviceable: " + package.Metadata.Serviceable);
            if (package.Metadata.Summary != null) logger.Info("  Summary: " + package.Metadata.Summary);
            if (package.Metadata.Tags != null) logger.Info("  Tags: " + package.Metadata.Tags);
            if (package.Metadata.Title != null) logger.Info("  Title: " + package.Metadata.Title);

            logger.Info("  Package framework references: " + (package.Metadata.FrameworkReferences.Any() ? string.Empty : "None"));

            foreach (var reference in package.Metadata.FrameworkReferences)
            {
                logger.Info("    Assembly: " + reference.AssemblyName);
                foreach (var framework in reference.SupportedFrameworks)
                {
                    logger.Info("      Framework: " + framework);
                }
            }

            logger.Info("  Package assembly references: " + (package.Metadata.PackageAssemblyReferences.Any() ? string.Empty : "None"));
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
                logger.Info("    Assembly: " + group.Key);
                foreach (var framework in group)
                {
                    logger.Info("      Framework: " + framework);
                }
            }

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
                            excludeLookup[@group.Key].SelectMany(
                                e => PathExtensions.Match(@group.Select(g => g.PackageIdentity.Id), e)));
                    }

                    if (excludeLookup.Contains(NuGetFramework.AnyFramework))
                    {
                        bannedPackages.AddRange(
                            excludeLookup[NuGetFramework.AnyFramework].SelectMany(
                                e => PathExtensions.Match(@group.Select(g => g.PackageIdentity.Id), e)));
                    }

                    foreach (var reference in group)
                    {
                        if (bannedPackages.Contains(reference.PackageIdentity.Id)) continue;
                        package.Metadata.AddDependency(reference);
                    }
                }
            }

            logger.Info("  Package dependencies: " + (package.Metadata.DependencyGroups.Any() ? string.Empty : "None"));
            foreach (var dependencyGroup in package.Metadata.DependencyGroups)
            {
                logger.Info("    Dependency group: " + dependencyGroup.TargetFramework);
                foreach (var dependency in dependencyGroup.Packages)
                {
                    logger.Info("      Dependency: " + dependency);
                }
            }

            try
            {
                var build = new PackageBuilder();
                build.Populate(package.Metadata);
                var includeLookup = package.IncludeFiles
                                           .SelectMany(include => PathExtensions.EnumerateFiles(include.Source)
                                                                                .Select(f => new
                                                                                {
                                                                                    include.Destination,
                                                                                    Source = f
                                                                                }))
                                           .ToLookup(p => p.Destination.ToLowerInvariant(), p => p.Source);

                var excludeLookup = package.ExcludeFiles.ToLookup(p => p.Destination.ToLowerInvariant(), p => p.Source);
                logger.Info("  Package files: " + (includeLookup.Any() ? string.Empty : "None"));

                foreach (var group in includeLookup)
                {
                    logger.Info("    Group: " + group.Key);
                    var bannedFiles = new List<string>();
                    if (excludeLookup.Contains(group.Key))
                    {
                        bannedFiles = excludeLookup[@group.Key].SelectMany(e => PathExtensions.Match(@group, e)).ToList();
                    }

                    foreach (var file in group)
                    {
                        if (!bannedFiles.Contains(file))
                        {
                            build.AddFiles(targetDirectory, file, group.Key);
                            logger.Info("      + File: " + file);
                        }
                        else
                        {
                            logger.Info("      - File: " + file);
                        }
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

        #endregion
    }
}