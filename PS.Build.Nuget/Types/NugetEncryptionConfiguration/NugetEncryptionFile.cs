using System.Xml.Serialization;

namespace PS.Build.Nuget.Types
{
    [XmlRoot("file")]
    public class NugetEncryptionFile
    {
        #region Properties

        [XmlElement("encrypted")]
        public string EncryptedHash { get; set; }

        [XmlElement("path")]
        public string Origin { get; set; }

        [XmlElement("original")]
        public string OriginalHash { get; set; }

        #endregion
    }
}