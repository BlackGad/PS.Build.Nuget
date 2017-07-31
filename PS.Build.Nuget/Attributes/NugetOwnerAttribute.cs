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
    public sealed class NugetOwnerAttribute : BaseNugetAttribute
    {
        private readonly string _owner;

        #region Constructors

        public NugetOwnerAttribute(string owner)
        {
            if (string.IsNullOrWhiteSpace(owner)) throw new ArgumentNullException("owner");
            _owner = owner;
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();

            try
            {
                logger.Debug("Defining nuget package owners");
                var package = provider.GetVaultPackage(ID);
                package.Metadata.AddOwner(_owner);
            }
            catch (Exception e)
            {
                logger.Error("Package owners definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}