using System;
using System.Diagnostics;
using NuGet.Frameworks;
using PS.Build.Extensions;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Nuget.Types
{
    class NugetEnvironment
    {
        #region Constructors

        public NugetEnvironment(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var explorer = provider.GetService<IExplorer>();
            var targetFrameworkVersionString = explorer.Properties[BuildProperty.TargetFrameworkVersion]?.Replace("v", string.Empty);
            Version targetFrameworkVersion;
            if (!Version.TryParse(targetFrameworkVersionString, out targetFrameworkVersion))
            {
                PackageFramework = NuGetFramework.AnyFramework;
                logger.Debug(" Target framework version is invalid");
            }
            else
            {
                PackageFramework = new NuGetFramework(FrameworkConstants.FrameworkIdentifiers.Net, targetFrameworkVersion);
            }
        }

        #endregion

        #region Properties

        public NuGetFramework PackageFramework { get; }

        #endregion
    }
}