using System;
using System.Diagnostics;
using PS.Build.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes.Base
{
    public abstract class BaseNugetAttribute : Attribute
    {
        #region Static members

        private static void Setup(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            logger.Info("Initializing NuGet attributes environment");

            var vault = provider.GetService<IDynamicVault>();
            if (vault == null) throw new ArgumentNullException(nameof(vault));

            var environment = vault.Query(() => new NugetEnvironment(provider));
            using (logger.IndentMessages())
            {
                logger.Debug($"* Package framework directory: {environment.PackageFramework.GetShortFolderName()}");
            }

            logger.Debug("Extending macro resolver service");
            provider.GetService<IMacroResolver>().Register(new NugetExtensionMacroHandler
            {
                PackageFrameworkDirectory = environment.PackageFramework.GetShortFolderName()
            });
        }

        #endregion

        #region Properties

        public string ID { get; set; }

        #endregion
    }
}