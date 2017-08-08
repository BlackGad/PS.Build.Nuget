using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    /// <summary>
    ///     Defines framework reference to be included to NuGet package
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetFrameworkReferenceAttribute : BaseNugetAttribute
    {
        private readonly string _assembly;
        private readonly string[] _frameworks;

        #region Constructors

        /// <summary>
        ///     Adds framework reference to NuGet package.
        /// </summary>
        /// <param name="assembly"> The fully qualified assembly name.</param>
        /// <param name="frameworks">
        ///     Specifies the target frameworks to which this reference applies. If omitted, indicates that
        ///     the reference applies to all frameworks.
        /// </param>
        public NugetFrameworkReferenceAttribute(string assembly, params string[] frameworks)
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
                package.Metadata.FrameworkReferences = package.Metadata.FrameworkReferences ?? new List<FrameworkAssemblyReference>();

                var frameworkReferences = (ICollection<FrameworkAssemblyReference>)package.Metadata.FrameworkReferences;
                frameworkReferences.Add(new FrameworkAssemblyReference(_assembly, _frameworks.Select(NuGetFramework.Parse).ToList()));
            }
            catch (Exception e)
            {
                logger.Error("Package framework assembly definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}