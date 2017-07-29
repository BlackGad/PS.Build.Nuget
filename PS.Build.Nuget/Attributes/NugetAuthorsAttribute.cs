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
    public sealed class NugetAuthorsAttribute : Attribute
    {
        private readonly string[] _authors;
        private readonly string _id;

        #region Constructors

        public NugetAuthorsAttribute(string id, params string[] authors)
        {
            if (authors == null) throw new ArgumentNullException("authors");
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            _id = id;
            _authors = authors;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            if (_authors == null || _authors.Length == 0) return;

            try
            {
                logger.Debug("Defining nuget package authors");
                var package = provider.GetService<IDynamicVault>().GetVaultPackage(_id);
                package.Metadata.Authors = package.Metadata.Authors ?? new List<string>();

                var authors = (ICollection<string>)package.Metadata.Authors;
                foreach (var author in _authors)
                {
                    authors.Add(author);
                }
            }
            catch (Exception e)
            {
                logger.Error("Package authors definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}