using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly X509FindType _findType;
        private readonly object _findValue;
        private readonly string _password;
        private readonly string _resourceName;

        private readonly Type _searchType;
        private readonly StoreLocation _storeLocation;
        private readonly StoreName _storeName;

        #region Constructors

        public NugetFilesEncryptionCertificateAttribute(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            _storeLocation = storeLocation;
            _storeName = storeName;
            _findType = findType;
            _findValue = findValue;
            
            _searchType = typeof(X509CertificateStorageSearch);

            Export = true;
        }

        public NugetFilesEncryptionCertificateAttribute(string certificateFilePath, string password)
        {
            _certificateFilePath = certificateFilePath;
            _password = password;

            _searchType = typeof(X509CertificateFileSearch);

            Export = true;
        }

        public NugetFilesEncryptionCertificateAttribute(string assemblyFilePath, string resourceName, string password)
        {
            _assemblyFilePath = assemblyFilePath;
            _resourceName = resourceName;
            _password = password;

            _searchType = typeof(X509CertificateManifestResourceSearch);

            Export = true;
        }

        #endregion

        #region Properties

        public string TargetCertificatePassword { get; set; }
        public bool Export { get; set; }

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

                IX509CertificateSearch search = null;
                if (_searchType == typeof(X509CertificateManifestResourceSearch))
                {
                    search = new X509CertificateManifestResourceSearch
                    {
                        Assembly = Assembly.LoadFile(resolver.Resolve(_assemblyFilePath)),
                        ResourceName = resolver.Resolve(_resourceName),
                        Password = _password
                    };
                }
                else if (_searchType == typeof(X509CertificateFileSearch))
                {
                    var path = resolver.Resolve(_certificateFilePath);
                    if (!File.Exists(path))
                    {
                        var newCertificate = X509Certificate2CreateExtensions.CreateSelfSignedCertificate("CN=" + package.Metadata.Id, "CN=PS.Build.Nuget");
                        var bytes = string.IsNullOrWhiteSpace(_password)
                            ? newCertificate.Export(X509ContentType.Pfx)
                            : newCertificate.Export(X509ContentType.Pfx, _password);

                        File.WriteAllBytes(path, bytes);
                    }

                    search = new X509CertificateFileSearch
                    {
                        SourceFile = path,
                        Password = _password
                    };
                }
                else if (_searchType == typeof(X509CertificateStorageSearch))
                {
                    search = new X509CertificateStorageSearch
                    {
                        StoreLocation = _storeLocation,
                        StoreName = _storeName,
                        FindType = _findType,
                        FindValue = _findValue
                    };
                }

                var certificate = search?.Search().FirstOrDefault();
                if (certificate == null) throw new ArgumentException("Certificate for files encrypting not found");

                package.X509Certificate = certificate;
                package.X509CertificatePassword = TargetCertificatePassword;
                package.X509CertificateExport = Export;
            }
            catch (Exception e)
            {
                logger.Error("Package file definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}