using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using PS.Build.Nuget.Extensions;

namespace PS.Build.Nuget.X509Certificate
{
    [XmlRoot("assembly")]
    public class X509CertificateManifestResourceSearch : X509CertificateStreamSearch
    {
        #region Properties

        [Display(Name = "Assembly",
            Order = 10,
            GroupName = "Generic",
            Description = "The Assembly from which to get embedded resource")]
        [XmlIgnore]
        public Assembly Assembly { get; set; }

        [Display(AutoGenerateField = false)]
        [XmlAttribute("assembly")]
        public string SourceAssembly { get; set; }

        [Display(
            Name = "Resource name",
            Order = 20,
            GroupName = "Generic",
            Description = "Assembly embedded resource name")]
        [XmlAttribute("resource")]
        public string ResourceName { get; set; }

        #endregion

        #region Override members

        protected override Stream GetStream()
        {
            var assembly = Assembly;
            if(assembly == null && !string.IsNullOrWhiteSpace(SourceAssembly)) assembly = Assembly.LoadFrom(SourceAssembly);
            if(assembly == null) throw new FileNotFoundException("Assembly not set");

            var name = assembly.ResolveResourceName(ResourceName);
            if (name == null) throw new FileNotFoundException($"Could not find appropriate embedded resource '{ResourceName}'");

            return assembly.GetManifestResourceStream(name);
        }

        #endregion
    }
}