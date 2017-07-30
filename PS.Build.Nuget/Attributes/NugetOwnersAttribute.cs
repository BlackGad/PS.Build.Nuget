using System;
using System.Collections.Generic;
using System.ComponentModel;
using PS.Build.Extensions;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetOwnersAttribute : Attribute
    {
        private readonly string _id;
        private readonly string[] _owners;

        #region Constructors

        public NugetOwnersAttribute(string id, params string[] owners)
        {
            if (owners == null) throw new ArgumentNullException("owners");
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            _id = id;
            _owners = owners;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            if (_owners == null || _owners.Length == 0) return;

            try
            {
                logger.Debug("Defining nuget package owners");
                var package = provider.GetVaultPackage(_id);
                package.Metadata.Owners = package.Metadata.Owners as List<string> ?? new List<string>();

                var owners = (ICollection<string>)package.Metadata.Owners;
                foreach (var owner in _owners)
                {
                    owners.Add(owner);
                }
            }
            catch (Exception e)
            {
                logger.Error("Package owners definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}