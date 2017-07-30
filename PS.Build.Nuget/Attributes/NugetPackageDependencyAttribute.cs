using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using PS.Build.Extensions;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetPackageDependencyAttribute : Attribute
    {
        #region Static members

        internal static void AddDependency(ManifestMetadata metadata, string dependencyID, string versionRange, NuGetFramework framework)
        {
            metadata.DependencyGroups = metadata.DependencyGroups ?? new List<PackageDependencyGroup>();

            var dependencyGroups = (ICollection<PackageDependencyGroup>)metadata.DependencyGroups;
            framework = framework ?? NuGetFramework.AnyFramework;

            var group = dependencyGroups.FirstOrDefault(g => g.TargetFramework.Equals(framework));
            if (@group == null)
            {
                @group = new PackageDependencyGroup(framework, new List<PackageDependency>());
                dependencyGroups.Add(@group);
            }

            var groupPackages = (ICollection<PackageDependency>)@group.Packages;
            var nugetVersionRange = VersionRange.All;
            if (versionRange != null) nugetVersionRange = VersionRange.Parse(versionRange);
            groupPackages.Add(new PackageDependency(dependencyID, nugetVersionRange));
        }

        #endregion

        private readonly string _dependencyID;
        private readonly string _framework;
        private readonly string _id;
        private readonly string _versionRange;

        #region Constructors

        public NugetPackageDependencyAttribute(string id, string dependencyID, string versionRange = null, string framework = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            if (string.IsNullOrWhiteSpace(dependencyID)) throw new ArgumentException("Invalid dependencyID");
            _id = id;
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

                var package = provider.GetVaultPackage(_id);
                var framework = NuGetFramework.AnyFramework;
                if (!string.IsNullOrWhiteSpace(_framework)) framework = NuGetFramework.Parse(_framework);
                AddDependency(package.Metadata, _dependencyID, _versionRange, framework);
            }
            catch (Exception e)
            {
                logger.Error("Package dependency definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}