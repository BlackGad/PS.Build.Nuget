using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using PS.Build.Extensions;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetFrameworkReferenceAttribute : Attribute
    {
        private readonly string _assembly;
        private readonly string[] _frameworks;
        private readonly string _id;

        #region Constructors

        public NugetFrameworkReferenceAttribute(string id, string assembly, params string[] frameworks)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (frameworks == null) throw new ArgumentNullException("frameworks");
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            _id = id;
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

                var package = provider.GetService<IDynamicVault>().GetVaultPackage(_id);
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