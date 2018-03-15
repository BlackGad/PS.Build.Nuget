using System.Collections.Generic;
using System.Xml.Serialization;

namespace PS.Build.Nuget.Types
{
    [XmlRoot("package")]
    public class NugetDebugPackage
    {
        #region Constructors

        public NugetDebugPackage()
        {
            Solutions = new List<NugetDebugSolution>();
        }

        #endregion

        #region Properties

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlElement("solution")]
        public List<NugetDebugSolution> Solutions { get; set; }

        #endregion
    }
}