using System;
using System.ComponentModel;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    /// <summary>
    ///     Defines files to be filtered from included in NuGet package
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetFilesFilterAttribute : BaseNugetAttribute
    {
        private readonly string _destination;
        private readonly string _source;

        #region Constructors

        /// <summary>
        ///     The location of the file or files to filter. The path is relative to the .nuspec file unless an absolute path is
        ///     specified. Macro resolver service
        ///     and wildcards '*', '**' and '?' are supported.
        /// </summary>
        /// <param name="source">Source pattern. All macro and wildcards '*', '**' and '?' are supported.</param>
        /// <param name="destination">
        ///     The relative path to the folder within the package where the source files are placed, which
        ///     could begin with lib, content, build, or tools. Macro resolver service and wildcards '*', '**' and '?' are
        ///     supported.
        /// </param>
        public NugetFilesFilterAttribute(string source, string destination)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            _source = source;
            _destination = destination;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();

            try
            {
                logger.Debug("Defining nuget package file filter");
                var package = provider.GetVaultPackage(ID);
                var resolver = provider.GetService<IMacroResolver>();
                package.ExcludeFiles.Add(new NugetPackageFiles(resolver.Resolve(_source),
                                                               resolver.Resolve(_destination)));
            }
            catch (Exception e)
            {
                logger.Error("Package file filter definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}