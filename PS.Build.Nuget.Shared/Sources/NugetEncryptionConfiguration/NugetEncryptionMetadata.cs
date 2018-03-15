using System.Xml.Serialization;

namespace PS.Build.Nuget.Shared.Sources
{
    [XmlRoot("metadata")]
    public class NugetEncryptionMetadata
    {
        #region Properties

        [XmlElement("certificate")]
        public string Certificate { get; set; }

        [XmlElement("key")]
        public string Key { get; set; }

        #endregion
    }
}