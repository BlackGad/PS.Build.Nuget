using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.X509Certificate;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    /// <summary>
    ///     Defines certificate for files encryption
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetFilesEncryptionCertificateAttribute : BaseNugetAttribute
    {
        private readonly string _assemblyFilePath;
        private readonly string _certificateFilePath;
        private readonly object _findValue;
        private readonly string _password;
        private readonly string _resourceName;

        private readonly Type _searchType;
        private readonly StoreLocation _storeLocation;
        private readonly StoreName _storeName;
        private readonly X509FindType _findType;

        #region Constructors

        public NugetFilesEncryptionCertificateAttribute(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            _storeLocation = storeLocation;
            _storeName = storeName;
            _findType = findType;
            _findValue = findValue;

            _searchType = typeof(X509CertificateStorageSearch);
        }

        public NugetFilesEncryptionCertificateAttribute(string certificateFilePath, string password)
        {
            _certificateFilePath = certificateFilePath;
            _password = password;

            _searchType = typeof(X509CertificateFileSearch);
        }

        public NugetFilesEncryptionCertificateAttribute(string assemblyFilePath, string resourceName, string password)
        {
            _assemblyFilePath = assemblyFilePath;
            _resourceName = resourceName;
            _password = password;

            _searchType = typeof(X509CertificateManifestResourceSearch);
        }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var resolver = provider.GetService<IMacroResolver>();

            try
            {
                logger.Debug("Searching security certificate");
                var package = provider.GetVaultPackage(ID);

                X509Certificate2 certificate = null;

                if (_searchType == typeof(X509CertificateManifestResourceSearch))
                {
                    var search = new X509CertificateManifestResourceSearch
                    {
                        Password = _password
                    };
                    certificate = search.Search().FirstOrDefault();
                }
                else if (_searchType == typeof(X509CertificateFileSearch))
                {
                }
                else if (_searchType == typeof(X509CertificateStorageSearch))
                {
                    var search = new X509CertificateStorageSearch
                    {
                        StoreLocation = _storeLocation,
                        StoreName = _storeName,
                        FindType = _findType,
                        FindValue = _findValue
                    };
                    certificate = search.Search().FirstOrDefault();
                }

                if (certificate == null) throw new ArgumentException("Certificate for files encrypting not found");

                package.X509Certificate = certificate;
            }
            catch (Exception e)
            {
                logger.Error("Package file definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}