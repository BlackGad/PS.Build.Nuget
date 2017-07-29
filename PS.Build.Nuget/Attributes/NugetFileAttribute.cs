using System;
using System.ComponentModel;
using PS.Build.Extensions;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetFileAttribute : Attribute
    {
        private readonly string _destination;
        private readonly string _exclude;
        private readonly string _id;
        private readonly string _source;

        #region Constructors

        public NugetFileAttribute(string id, string source, string destination, string exclude = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            _id = id;
            _source = source;
            _destination = destination;
            _exclude = exclude;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();

            try
            {
                logger.Debug("Defining nuget package file");
                var package = provider.GetService<IDynamicVault>().GetVaultPackage(_id);
                package.Files.Add(new NugetPackageFiles(_source, _destination, _exclude));
            }
            catch (Exception e)
            {
                logger.Error("Package file definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}