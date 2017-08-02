using System.Xml.Serialization;

namespace PS.Build.Nuget.Types
{
    [XmlRoot("solution")]
    public class NugetDebugSolution
    {
        #region Properties

        [XmlAttribute("dir")]
        public string Directory { get; set; }

        #endregion
    }
}