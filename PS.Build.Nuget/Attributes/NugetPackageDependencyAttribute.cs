using System;
using System.ComponentModel;
using System.Reflection;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetPackageDependencyAttribute : BaseNugetAttribute
    {
        private readonly NugetPackageDependencyBehavior _behavior;
        private readonly string _dependencyID;
        private readonly string _framework;
        private readonly string _versionRange;

        #region Constructors

        public NugetPackageDependencyAttribute(string dependencyID, string versionRange, string framework = null)
        {
            if (string.IsNullOrWhiteSpace(dependencyID)) throw new ArgumentException("Invalid dependencyID");
            _dependencyID = dependencyID;
            _versionRange = versionRange;
            _framework = framework;
        }

        public NugetPackageDependencyAttribute(string dependencyID,
                                               NugetPackageDependencyBehavior behavior = NugetPackageDependencyBehavior.None,
                                               string framework = null)
        {
            if (string.IsNullOrWhiteSpace(dependencyID)) throw new ArgumentException("Invalid dependencyID");
            _dependencyID = dependencyID;
            _behavior = behavior;
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

                VersionRange versionRange = null;
                NuGetVersion nugetVersion = null;

                if (string.IsNullOrEmpty(_versionRange))
                {
                    var compilation = provider.GetService<Microsoft.CodeAnalysis.CSharp.CSharpCompilation>();
                    var versionAttribute = compilation.Assembly.GetAttribute<AssemblyVersionAttribute>();
                    Version version;
                    if (Version.TryParse(versionAttribute?.Version ?? string.Empty, out version))
                    {
                        nugetVersion = NuGetVersion.Parse(version.ToString());
                        switch (_behavior)
                        {
                            case NugetPackageDependencyBehavior.None:
                                versionRange = new VersionRange(nugetVersion, new FloatRange(NuGetVersionFloatBehavior.None, nugetVersion));
                                break;
                            case NugetPackageDependencyBehavior.Major:
                                versionRange = new VersionRange(nugetVersion, new FloatRange(NuGetVersionFloatBehavior.Major, nugetVersion));
                                break;
                            case NugetPackageDependencyBehavior.Minor:
                                versionRange = new VersionRange(nugetVersion, new FloatRange(NuGetVersionFloatBehavior.Minor, nugetVersion));
                                break;
                            case NugetPackageDependencyBehavior.Patch:
                                versionRange = new VersionRange(nugetVersion, new FloatRange(NuGetVersionFloatBehavior.Patch, nugetVersion));
                                break;
                            case NugetPackageDependencyBehavior.Revision:
                                versionRange = new VersionRange(nugetVersion, new FloatRange(NuGetVersionFloatBehavior.Revision, nugetVersion));
                                break;
                        }
                    }
                }
                else
                {
                    VersionRange.TryParse(_versionRange ?? string.Empty, out versionRange);
                    NuGetVersion.TryParse(_versionRange ?? string.Empty, out nugetVersion);
                }

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