using System.Xml.Serialization;
using PS.Build.Nuget.X509Certificate;

namespace PS.Build.Nuget.Shared.Sources
{
    [XmlRoot("package")]
    public class NugetCertificatePackage
    {
        #region Properties

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlElement("storage", typeof(X509CertificateStorageSearch))]
        [XmlElement("file", typeof(X509CertificateFileSearch))]
        [XmlElement("resource", typeof(X509CertificateManifestResourceSearch))]
        public X509CertificateSearch Search { get; set; }

        #endregion
    }
}