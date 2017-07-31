using System;
using System.ComponentModel;
using NuGet.Frameworks;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetPackageDependenciesFilterAttribute : BaseNugetAttribute
    {
        private readonly string _dependencyIDMask;
        private readonly string _framework;

        #region Constructors

        public NugetPackageDependenciesFilterAttribute(string dependencyIDMask, string framework = null)
        {
            if (string.IsNullOrWhiteSpace(dependencyIDMask)) throw new ArgumentException("Invalid dependencyID mask");
            _dependencyIDMask = dependencyIDMask;
            _framework = framework;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();

            try
            {
                logger.Debug("Defining nuget package dependency filter");

                var package = provider.GetVaultPackage(ID);
                var framework = NuGetFramework.AnyFramework;
                if (!string.IsNullOrWhiteSpace(_framework)) framework = NuGetFramework.Parse(_framework);
                package.ExcludeDependencies.Add(new NugetPackageDependencyFilter(_dependencyIDMask, framework));
            }
            catch (Exception e)
            {
                logger.Error("Package dependency definition filter failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}