using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace PS.Build.Nuget.X509Certificate
{
    public class X509CertificateStorageIdentifier : IFormattable
    {
        #region Static members

        public static bool IsThumbprintValid(string thumbprint)
        {
            thumbprint = thumbprint ?? string.Empty;
            thumbprint = thumbprint.ToLowerInvariant();
            thumbprint = thumbprint.Replace(" ", "");
            thumbprint = thumbprint.Replace("-", "");
            if (thumbprint.Any(c => !(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f'))) return false;
            if (thumbprint.Length != 40) return false;
            return true;
        }

        public static X509CertificateStorageIdentifier Parse(string value)
        {
            X509CertificateStorageIdentifier result;
            if (TryParse(value, out result)) return result;
            throw new FormatException($"Could not parse {value} to {typeof(X509CertificateStorageIdentifier).Name}");
        }

        public static bool TryParse(string value, out X509CertificateStorageIdentifier result)
        {
            result = null;
            string pattern = $@"^(?<{nameof(StoreLocation)}>[^\\]+)\\(?<{nameof(StoreName)}>[^:]+):(?<{nameof(Thumbprint)}>.+)$";
            var match = Regex.Match(value, pattern);
            if (!match.Success) return false;

            StoreLocation storeLocation;
            if (!Enum.TryParse(match.Groups[nameof(StoreLocation)].Value, true, out storeLocation)) return false;

            StoreName storeName;
            if (!Enum.TryParse(match.Groups[nameof(StoreName)].Value, true, out storeName)) return false;

            var thumbprint = match.Groups[nameof(Thumbprint)].Value;
            if (!IsThumbprintValid(thumbprint)) return false;

            result = new X509CertificateStorageIdentifier(storeLocation, storeName, thumbprint);
            return true;
        }

        private static string GetThumbprint(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }

        #endregion

        #region Constructors

        public X509CertificateStorageIdentifier(StoreLocation location, StoreName name, string thumbprint)
        {
            if (!IsThumbprintValid(thumbprint)) throw new ArgumentException("Thumbprint is not valid");
            Thumbprint = thumbprint;
            StoreLocation = location;
            StoreName = name;
        }

        public X509CertificateStorageIdentifier(StoreLocation location, StoreName name, byte[] thumbprintHash)
            : this(location, name, GetThumbprint(thumbprintHash))
        {
        }

        #endregion

        #region Properties

        public StoreLocation StoreLocation { get; }

        public StoreName StoreName { get; }

        public string Thumbprint { get; }

        #endregion

        #region Override members

        public override string ToString()
        {
            return ((IFormattable)this).ToString(null, null);
        }

        #endregion

        #region IFormattable Members

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return $"{StoreLocation}\\{StoreName}: {Thumbprint}";
        }

        #endregion

        #region Members

        public X509Certificate2 GetCertificate()
        {
            var certificateSearch = new X509CertificateStorageSearch
            {
                FindType = X509FindType.FindByThumbprint,
                FindValue = Thumbprint,
                StoreLocation = StoreLocation,
                StoreName = StoreName
            };
            return certificateSearch.Search().FirstOrDefault();
        }

        public byte[] GetThumbprintHash()
        {
            try
            {
                return Enumerable.Range(0, Thumbprint.Length)
                                 .Where(x => x%2 == 0)
                                 .Select(x => Convert.ToByte(Thumbprint.Substring(x, 2), 16))
                                 .ToArray();
            }
            catch
            {
                // ReSharper disable once ThrowFromCatchWithNoInnerException
                throw new InvalidDataException($"'{Thumbprint}' hash is invalid");
            }
        }

        #endregion
    }
}