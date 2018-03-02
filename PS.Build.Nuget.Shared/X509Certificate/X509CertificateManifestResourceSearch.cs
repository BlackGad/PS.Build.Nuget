using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using PS.Build.Nuget.Extensions;

namespace PS.Build.Nuget.X509Certificate
{
    public class X509CertificateManifestResourceSearch : X509CertificateStreamSearch
    {
        #region Properties

        [Display(Name = "Assembly",
            Order = 10,
            GroupName = "Generic",
            Description = "The Assembly from which to get embedded resource")]
        public Assembly Assembly { get; set; }

        [Display(
            Name = "Resource name",
            Order = 20,
            GroupName = "Generic",
            Description = "Assembly embedded resource name")]
        public string ResourceName { get; set; }

        #endregion

        #region Override members

        protected override Stream GetStream()
        {
            var name = Assembly.ResolveResourceName(ResourceName);
            if (name == null) throw new FileNotFoundException($"Could not find appropriate embedded resource '{ResourceName}'");

            return Assembly.GetManifestResourceStream(name);
        }

        #endregion
    }
}