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
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
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
                var package = provider.GetVaultPackage(_id);
                var resolver = provider.GetService<IMacroResolver>();
                var exclude = _exclude;
                if (exclude != null) exclude = resolver.Resolve(exclude);
                package.Files.Add(new NugetPackageFiles(resolver.Resolve(_source),
                                                        resolver.Resolve(_destination),
                                                        exclude));
            }
            catch (Exception e)
            {
                logger.Error("Package file definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}