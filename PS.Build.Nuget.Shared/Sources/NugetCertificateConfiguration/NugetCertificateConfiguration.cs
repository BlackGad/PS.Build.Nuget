using System.Collections.Generic;
using System.Xml.Serialization;
using PS.Build.Nuget.X509Certificate;

namespace PS.Build.Nuget.Shared.Sources
{
    [XmlRoot("certificates")]
    public class NugetCertificateConfiguration
    {
        #region Properties

        public NugetCertificateConfiguration()
        {
            Packages = new List<NugetCertificatePackage>();
        }

        [XmlElement("package")]
        public List<NugetCertificatePackage> Packages { get; set; }

        #endregion
    }
}