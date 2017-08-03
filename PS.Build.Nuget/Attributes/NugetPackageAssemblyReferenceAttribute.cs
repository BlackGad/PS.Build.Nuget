using System;
using System.ComponentModel;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    /// <summary>
    ///     Defines package assembly reference to be set in NuGet package
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetPackageAssemblyReferenceAttribute : BaseNugetAttribute
    {
        private readonly string _assembly;
        private readonly string[] _frameworks;

        #region Constructors

        /// <summary>
        ///     Add internal NuGet package assembly reference.
        /// </summary>
        /// <param name="assembly">Package assembly filename.</param>
        /// <param name="frameworks">
        ///     Specifies the target frameworks to which this reference applies. If omitted, indicates that
        ///     the reference applies to all frameworks.
        /// </param>
        public NugetPackageAssemblyReferenceAttribute(string assembly, params string[] frameworks)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (frameworks == null) throw new ArgumentNullException("frameworks");
            _assembly = assembly;
            _frameworks = frameworks;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();

            try
            {
                logger.Debug("Defining nuget package framework assembly");

                var package = provider.GetVaultPackage(ID);
                var assembly = provider.GetService<IMacroResolver>().Resolve(_assembly);
                package.Metadata.AddAssemblyReference(assembly, _frameworks);
            }
            catch (Exception e)
            {
                logger.Error("Package framework assembly definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}