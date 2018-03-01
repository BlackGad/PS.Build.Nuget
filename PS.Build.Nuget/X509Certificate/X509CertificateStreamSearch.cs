using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PS.Build.Nuget.X509Certificate
{
    public abstract class X509CertificateStreamSearch : IX509CertificateSearch
    {
        #region Static members

        private static byte[] ReadStream(Stream input)
        {
            var buffer = new byte[16*1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        #endregion

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
                return new[] { new X509Certificate2(ReadStream(stream), Password) };
            }
        }

        #endregion

        #region Members

        protected abstract Stream GetStream();

        #endregion
    }
}