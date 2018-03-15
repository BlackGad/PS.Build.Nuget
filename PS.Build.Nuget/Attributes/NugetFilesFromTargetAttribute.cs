using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Nuget.Attributes
{
    /// <summary>
    ///     Defines default target files to be included to NuGet package.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetFilesFromTargetAttribute : BaseNugetAttribute
    {
        #region Constructors

        /// <summary>
        ///     Automatically add project target files to NuGet package.
        /// </summary>
        public NugetFilesFromTargetAttribute()
        {
            MarkTargetAsAssemblyReference = true;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Encrypt files before packing.
        /// </summary>
        public bool Encrypt { get; set; }

        /// <summary>
        ///     Add target xml documentation as well.
        /// </summary>
        public bool IncludeDocumentation { get; set; }

        /// <summary>
        ///     Add target pdb as well.
        /// </summary>
        public bool IncludePDB { get; set; }

        /// <summary>
        ///     Set target assembly as NuGet package assembly reference.
        /// </summary>
        public bool MarkTargetAsAssemblyReference { get; set; }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var explorer = provider.GetService<IExplorer>();

            try
            {
                var package = provider.GetVaultPackage(ID);

                var target = explorer.Properties[BuildProperty.TargetPath];
                if (MarkTargetAsAssemblyReference)
                {
                    package.Metadata.AddAssemblyReference(Path.GetFileName(target), null);
                }

                var files = new List<string>
                {
                    target
                };

                if (IncludePDB)
                    files.Add(Path.Combine(explorer.Directories[BuildDirectory.Target], explorer.Properties[BuildProperty.TargetName] + ".pdb"));
                if (IncludeDocumentation)
                    files.Add(Path.Combine(explorer.Directories[BuildDirectory.Target], explorer.Properties[BuildProperty.TargetName] + ".xml"));

                var destinationDirectory = Path.Combine("lib",
                                                        provider.GetService<IDynamicVault>()
                                                                .Query<NugetEnvironment>()
                                                                .PackageFramework
                                                                .GetShortFolderName());

                foreach (var file in files)
                {
                    package.IncludeFiles.Add(new NugetPackageFiles(file, destinationDirectory, Encrypt));
                }
            }
            catch (Exception e)
            {
                logger.Error("Target files definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}