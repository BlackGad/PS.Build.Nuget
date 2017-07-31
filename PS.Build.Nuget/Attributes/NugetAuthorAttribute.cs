using System;
using System.ComponentModel;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetAuthorAttribute : BaseNugetAttribute
    {
        private readonly string _author;
        private readonly bool _isOwner;

        #region Constructors

        public NugetAuthorAttribute(string author, bool isOwner = true)
        {
            if (author == null) throw new ArgumentNullException("author");
            _author = author;
            _isOwner = isOwner;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            try
            {
                logger.Debug("Defining nuget package author");
                var package = provider.GetVaultPackage(ID);

                package.Metadata.AddAuthor(_author);
                if (_isOwner) package.Metadata.AddOwner(_author);
            }
            catch (Exception e)
            {
                logger.Error("Package authors definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}