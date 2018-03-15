using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using PS.Build.Nuget.Shared.Extensions;

namespace PS.Build.Nuget.X509Certificate
{
    public abstract class X509CertificateStreamSearch : IX509CertificateSearch
    {
        #region Properties

        [Display(
            Name = "Password",
            GroupName = "Generic",
            Order = 100,
            Description = "PFX container password")]
        public string Password { get; set; }

        #endregion

        #region IX509CertificateSearch Members

        public X509Certificate2[] Search()
        {
            using (var stream = GetStream())
            {
                return new[] { new X509Certificate2(stream.ReadStream(), Password, X509KeyStorageFlags.Exportable) };
            }
        }

        #endregion

        #region Members

        protected abstract Stream GetStream();

        #endregion
    }
}