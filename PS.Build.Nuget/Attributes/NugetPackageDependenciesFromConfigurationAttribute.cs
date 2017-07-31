using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
    public sealed class NugetPackageDependenciesFromConfigurationAttribute : BaseNugetAttribute
    {
        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var explorer = provider.GetService<IExplorer>();
            var logger = provider.GetService<ILogger>();

            var packagesConfig = explorer.Items[BuildItem.None].FirstOrDefault(i => string.Equals(i.Identity,
                                                                                                  "packages.config",
                                                                                                  StringComparison.InvariantCultureIgnoreCase));

            if (packagesConfig != null && File.Exists(packagesConfig.FullPath))
            {
                try
                {
                    logger.Info("Resolving package dependencies from packages.config");
                    var package = provider.GetVaultPackage(ID);
                    logger.Debug(" Reading...");
                    var configReader = new PackagesConfigReader(XDocument.Parse(File.ReadAllText(packagesConfig.FullPath)));
                    var dependencies = configReader.GetPackages(false);
                    foreach (var dependency in dependencies)
                    {
                        if (!dependency.IsUserInstalled)
                        {
                            logger.Debug($" - Dependency '{dependency.PackageIdentity.Id}' installed automatically as " +
                                         "dependecy for another package. Skipping.");
                            continue;
                        }
                        if (dependency.PackageIdentity.Id.StartsWith("PS.Build"))
                        {
                            logger.Debug($" - Dependency '{dependency.PackageIdentity.Id}' id starts from 'PS.Build'. Skipping.");
                            continue;
                        }

                        var versionRange = dependency.PackageIdentity.HasVersion ? dependency.PackageIdentity.Version.ToString() : null;
                        if (dependency.HasAllowedVersions) versionRange = dependency.AllowedVersions.ToString();

                        package.Metadata.AddDependency(dependency.PackageIdentity.Id,
                                                       versionRange,
                                                       NuGetFramework.AnyFramework);

                        logger.Debug($" + Nuget dependency '{dependency.PackageIdentity.Id}' added.");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(" Configuration file packages.config parse error. Details: " + e.GetBaseException().Message);
                }
            }
            else logger.Error(" Configuration file packages.config not found");
        }

        #endregion
    }
}