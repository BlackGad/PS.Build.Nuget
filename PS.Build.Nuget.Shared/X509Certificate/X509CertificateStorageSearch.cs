using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace PS.Build.Nuget.X509Certificate
{
    public class X509CertificateStorageSearch : X509CertificateSearch
    {
        #region Properties

        [Display(
            Name = "Find type",
            Order = 20,
            GroupName = "Generic",
            Description = "Identifies the type of value provided in the 'Find value' parameter")]
        [XmlAttribute("criteria")]
        public X509FindType FindType { get; set; }

        [Display(
            Name = "Find value",
            Order = 30,
            GroupName = "Generic",
            Description = "Value to search")]
        [XmlAttribute("value")]
        public string FindValue { get; set; }

        [Display(
            Name = "Store location",
            Order = 0,
            GroupName = "Generic",
            Description = "Specifies the location of the X.509 certificate store")]
        [XmlAttribute("location")]
        public StoreLocation StoreLocation { get; set; }

        [Display(
            Name = "Store",
            Order = 10,
            GroupName = "Generic",
            Description = "Specifies the name of the X.509 certificate store to open")]
        [XmlAttribute("name")]
        public StoreName StoreName { get; set; }

        #endregion

        #region X509CertificateSearch Members

        public override X509Certificate2[] Search()
        {
            var store = new X509Store(StoreName, StoreLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var availableCertificates = string.IsNullOrWhiteSpace(FindValue)
                    ? store.Certificates
                    : store.Certificates.Find(FindType, FindValue, false);

                return availableCertificates.OfType<X509Certificate2>().ToArray();
            }
            finally
            {
                store.Close();
            }
        }

        #endregion
    }
}