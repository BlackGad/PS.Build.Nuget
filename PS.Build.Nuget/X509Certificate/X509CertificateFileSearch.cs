using System.ComponentModel.DataAnnotations;
using System.IO;

namespace PS.Build.Nuget.X509Certificate
{
    public class X509CertificateFileSearch : X509CertificateStreamSearch
    {
        #region Properties

        [Display(
            Name = "PFX file source",
            Order = 10,
            GroupName = "Generic",
            Description = "PFX file source")]
        public string SourceFile { get; set; }

        #endregion

        #region Override members

        protected override Stream GetStream()
        {
            return File.OpenRead(SourceFile);
        }

        #endregion
    }
}