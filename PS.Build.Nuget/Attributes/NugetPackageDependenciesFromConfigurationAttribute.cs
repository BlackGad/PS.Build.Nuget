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
        #region Constructors

        public NugetPackageDependenciesFromConfigurationAttribute()
        {
            AllDependenciesForAnyFramework = true;
        }

        #endregion

        #region Properties

        public bool AllDependenciesForAnyFramework { get; set; }

        #endregion

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
                    var references = configReader.GetPackages(false);
                    foreach (var reference in references)
                    {
                        if (!reference.IsUserInstalled)
                        {
                            logger.Debug($" - Dependency '{reference.PackageIdentity.Id}' installed automatically as " +
                                         "dependency for another package. Skipping.");
                            continue;
                        }

                        if (AllDependenciesForAnyFramework)
                        {
                            package.IncludeDependencies.Add(new PackageReference(reference.PackageIdentity,
                                                                                 NuGetFramework.AnyFramework,
                                                                                 reference.IsUserInstalled,
                                                                                 reference.IsDevelopmentDependency,
                                                                                 reference.RequireReinstallation,
                                                                                 reference.AllowedVersions));
                        }
                        else
                        {
                            package.IncludeDependencies.Add(reference);
                        }

                        logger.Debug($" + Nuget dependency '{reference.PackageIdentity.Id}' added.");
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