using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace PS.Build.Nuget.Types
{
    [XmlRoot("configuration")]
    public class NugetDebugConfiguration
    {
        #region Constructors

        public NugetDebugConfiguration()
        {
            Packages = new List<NugetDebugPackage>();
        }

      

        #endregion

        #region Properties

        [XmlElement("package")]
        public List<NugetDebugPackage> Packages { get; set; }

        #endregion
    }

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

    [XmlRoot("solution")]
    public class NugetDebugSolution
    {
        #region Properties

        [XmlAttribute("dir")]
        public string Directory { get; set; }

        #endregion
    }
}