using System.Collections.Generic;
using System.Xml.Serialization;

namespace PS.Build.Nuget.Types
{
    [XmlRoot("configuration")]
    public class NugetDebugConfiguration
    {
        #region Static members

        public static NugetDebugConfiguration GetTemplate()
        {
            return new NugetDebugConfiguration
            {
                Packages = new List<NugetDebugPackage>
                {
                    new NugetDebugPackage
                    {
                        ID = "Package1.ID",
                        Solutions = new List<NugetDebugSolution>
                        {
                            new NugetDebugSolution
                            {
                                Directory = "solution dir 1"
                            },
                            new NugetDebugSolution
                            {
                                Directory = "solution dir 2"
                            }
                        }
                    },
                    new NugetDebugPackage
                    {
                        ID = "Package2.ID",
                        Solutions = new List<NugetDebugSolution>
                        {
                            new NugetDebugSolution
                            {
                                Directory = "solution dir 1"
                            }
                        }
                    }
                }
            };
        }

        #endregion

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
}