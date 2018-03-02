using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using PS.Build.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;
using PS.Build.Types;
using NugetPackage = PS.Build.Nuget.Types.NugetPackage;

namespace PS.Build.Nuget.Extensions
{
    internal static class ServiceProviderExtensions
    {
        #region Static members

        internal static void AddAssemblyReference(this ManifestMetadata metadata, string assembly, string[] frameworks)
        {
            metadata.PackageAssemblyReferences = metadata.PackageAssemblyReferences ?? new List<PackageReferenceSet>();

            var references = (ICollection<PackageReferenceSet>)metadata.PackageAssemblyReferences;

            if (frameworks?.Length > 0)
            {
                foreach (var framework in frameworks)
                {
                    references.Add(new PackageReferenceSet(NuGetFramework.Parse(framework), new[] { assembly }));
                }
            }
            else
            {
                references.Add(new PackageReferenceSet(NuGetFramework.AnyFramework, new[] { assembly }));
            }
        }

        internal static void AddAuthor(this ManifestMetadata metadata, string name)
        {
            metadata.Authors = metadata.Authors as List<string> ?? new List<string>();

            var collection = (ICollection<string>)metadata.Authors;
            collection.Add(name);
        }

        internal static void AddDependency(this ManifestMetadata metadata, PackageReference reference)
        {
            metadata.DependencyGroups = metadata.DependencyGroups ?? new List<PackageDependencyGroup>();

            var dependencyGroups = (ICollection<PackageDependencyGroup>)metadata.DependencyGroups;
            var framework = reference.TargetFramework ?? NuGetFramework.AnyFramework;

            var group = dependencyGroups.FirstOrDefault(g => g.TargetFramework.Equals(framework));
            if (@group == null)
            {
                @group = new PackageDependencyGroup(framework, new List<PackageDependency>());
                dependencyGroups.Add(@group);
            }

            var groupPackages = (ICollection<PackageDependency>)@group.Packages;

            var nugetVersionRange = VersionRange.All;
            if (reference.HasAllowedVersions) nugetVersionRange = reference.AllowedVersions;
            else if (reference.PackageIdentity.HasVersion) nugetVersionRange = new VersionRange(reference.PackageIdentity.Version);

            groupPackages.Add(new PackageDependency(reference.PackageIdentity.Id, nugetVersionRange));
        }

        internal static void AddOwner(this ManifestMetadata metadata, string name)
        {
            metadata.Owners = metadata.Owners as List<string> ?? new List<string>();

            var collection = (ICollection<string>)metadata.Owners;
            collection.Add(name);
        }

        internal static IEnumerable<NugetPackageFile> EnumerateFiles(this NugetPackage package, ILogger logger)
        {
            var includeLookup = package.IncludeFiles
                                       .SelectMany(include => PathExtensions.EnumerateFiles(include.Source)
                                                                            .Select(f => new
                                                                            {
                                                                                include.Destination,
                                                                                include.Encrypt,
                                                                                Source = f
                                                                            }))
                                       .ToLookup(p => p.Destination.ToLowerInvariant(), p => p);

            var excludeLookup = package.ExcludeFiles.ToLookup(p => p.Destination.ToLowerInvariant(), p => p.Source);
            logger?.Info("Package files: " + (includeLookup.Any() ? string.Empty : "None"));
            foreach (var group in includeLookup)
            {
                using (logger?.IndentMessages())
                {
                    logger?.Info("Group: " + group.Key);
                    var bannedFiles = new List<string>();
                    if (excludeLookup.Contains(group.Key))
                    {
                        bannedFiles = excludeLookup[@group.Key].SelectMany(e => PathExtensions.Match(@group.Select(g => g.Source), e)).ToList();
                    }

                    foreach (var file in group)
                    {
                        using (logger?.IndentMessages())
                        {
                            if (!bannedFiles.Contains(file.Source))
                            {
                                yield return new NugetPackageFile
                                {
                                    Source = file.Source,
                                    Destination = group.Key,
                                    Encrypt = file.Encrypt
                                };

                                logger?.Info("+ File: " + file.Source);
                            }
                            else
                            {
                                logger?.Info("- File: " + file.Source);
                            }
                        }
                    }
                }
            }
        }

        internal static NugetPackage GetVaultPackage(this IServiceProvider provider, string id)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (string.IsNullOrWhiteSpace(id)) id = provider.GetService<IExplorer>().Properties[BuildProperty.TargetName];

            var vault = provider.GetService<IDynamicVault>();
            if (vault == null) throw new ArgumentNullException(nameof(vault));

            return vault.Query(id.ToLowerInvariant(), () => CreateInitialPackage(provider, id));
        }

        private static NugetPackage CreateInitialPackage(IServiceProvider provider, string id)
        {
            var compilation = provider.GetService<Microsoft.CodeAnalysis.CSharp.CSharpCompilation>();
            var logger = provider.GetService<ILogger>();

            logger.Debug($"Creating initial '{id}' package record");

            var metadata = new ManifestMetadata
            {
                Id = id
            };
            using (logger.IndentMessages())
            {
                var versionAttribute = compilation.Assembly.GetAttribute<AssemblyVersionAttribute>();
                if (!string.IsNullOrEmpty(versionAttribute?.Version))
                {
                    logger.Debug("Assembly version attribute is: " + versionAttribute.Version);
                    NuGetVersion nugetVersion;
                    if (NuGetVersion.TryParse(versionAttribute.Version, out nugetVersion))
                    {
                        logger.Debug("+ Nuget package version: " + nugetVersion);
                        metadata.Version = nugetVersion;
                    }
                    else
                    {
                        logger.Debug("- Could not parse version");
                    }
                }
                else logger.Debug("Assembly version attribute is empty or not defined");

                var companyAttribute = compilation.Assembly.GetAttribute<AssemblyCompanyAttribute>();
                if (!string.IsNullOrEmpty(companyAttribute?.Company))
                {
                    logger.Debug("Assembly company attribute is: " + companyAttribute.Company);
                    metadata.Owners = new[] { companyAttribute.Company };
                    metadata.Authors = new[] { companyAttribute.Company };
                    logger.Debug("+ Nuget package owner: " + companyAttribute.Company);
                }
                else logger.Debug("Assembly company attribute is empty or not defined");

                var titleAttribute = compilation.Assembly.GetAttribute<AssemblyTitleAttribute>();
                if (!string.IsNullOrEmpty(titleAttribute?.Title))
                {
                    logger.Debug("Assembly title attribute is: " + titleAttribute.Title);
                    metadata.Title = titleAttribute.Title;
                    logger.Debug("+ Nuget package title: " + titleAttribute.Title);
                }
                else logger.Debug("Assembly title attribute is empty or not defined");

                var descriptionAttribute = compilation.Assembly.GetAttribute<AssemblyDescriptionAttribute>();
                if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
                {
                    logger.Debug("Assembly description attribute is: " + descriptionAttribute.Description);
                    metadata.Description = descriptionAttribute.Description;
                    logger.Debug("+ Nuget package description: " + descriptionAttribute.Description);
                }
                else logger.Debug("Assembly description attribute is empty or not defined");

                var copyrightAttribute = compilation.Assembly.GetAttribute<AssemblyCopyrightAttribute>();
                if (!string.IsNullOrEmpty(copyrightAttribute?.Copyright))
                {
                    logger.Debug("Assembly copyright attribute is: " + copyrightAttribute.Copyright);
                    metadata.Copyright = copyrightAttribute.Copyright;
                    logger.Debug("+ Nuget package copyright: " + copyrightAttribute.Copyright);
                }
                else logger.Debug("Assembly copyright attribute is empty or not defined");

                var cultureAttribute = compilation.Assembly.GetAttribute<AssemblyCultureAttribute>();
                if (!string.IsNullOrEmpty(cultureAttribute?.Culture))
                {
                    logger.Debug("Assembly culture attribute is: " + cultureAttribute.Culture);
                    metadata.Language = cultureAttribute.Culture;
                    logger.Debug("+ Nuget package language: " + cultureAttribute.Culture);
                }
                else logger.Debug("Assembly culture attribute is empty or not defined");
            }

            return new NugetPackage(metadata);
        }

        #endregion
    }
}