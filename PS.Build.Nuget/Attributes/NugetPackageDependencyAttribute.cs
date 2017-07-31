using System;
using System.ComponentModel;
using System.Diagnostics;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetPackageDependencyAttribute : BaseNugetAttribute
    {
        private readonly string _dependencyID;
        private readonly string _framework;
        private readonly string _versionRange;

        #region Constructors

        public NugetPackageDependencyAttribute(string dependencyID, string versionRange = null, string framework = null)
        {
            if (string.IsNullOrWhiteSpace(dependencyID)) throw new ArgumentException("Invalid dependencyID");
            _dependencyID = dependencyID;
            _versionRange = versionRange;
            _framework = framework;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();

            try
            {
                logger.Debug("Defining nuget package dependency");
                var package = provider.GetVaultPackage(ID);
                var framework = NuGetFramework.AnyFramework;
                if (!string.IsNullOrWhiteSpace(_framework)) framework = NuGetFramework.Parse(_framework);
                VersionRange versionRange;
                VersionRange.TryParse(_versionRange ?? string.Empty, out versionRange);
                NuGetVersion nugetVersion;
                NuGetVersion.TryParse(_versionRange ?? string.Empty, out nugetVersion);
                package.IncludeDependencies.Add(new PackageReference(new PackageIdentity(_dependencyID, nugetVersion),
                                                                     framework,
                                                                     false,
                                                                     false,
                                                                     false,
                                                                     versionRange));
            }
            catch (Exception e)
            {
                logger.Error("Package dependency definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}