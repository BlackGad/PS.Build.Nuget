using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml.Serialization;

namespace PS.Build.Nuget.X509Certificate
{
    [XmlRoot("file")]
    public class X509CertificateFileSearch : X509CertificateStreamSearch
    {
        #region Properties

        [Display(
            Name = "PFX file source",
            Order = 10,
            GroupName = "Generic",
            Description = "PFX file source")]
        [XmlAttribute("source")]
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