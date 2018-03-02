using System.Collections.Generic;
using System.Xml.Serialization;

namespace PS.Build.Nuget.Types
{
    [XmlRoot("configuration")]
    public class NugetEncryptionConfiguration
    {
        #region Constructors

        public NugetEncryptionConfiguration()
        {
            Files = new List<NugetEncryptionFile>();
        }

        #endregion

        #region Properties

        [XmlArrayItem("file")]
        [XmlArray("files")]
        public List<NugetEncryptionFile> Files { get; set; }

        [XmlElement("metadata")]
        public NugetEncryptionMetadata Metadata { get; set; }

        #endregion
    }
}