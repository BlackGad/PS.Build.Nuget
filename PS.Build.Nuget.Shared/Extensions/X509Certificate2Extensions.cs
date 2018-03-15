using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PS.Build.Nuget.Extensions
{
    public static class X509Certificate2Extensions
    {
        #region Static members

        public static byte[] Decrypt(this X509Certificate2 certificate, byte[] encryptedData)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));

            var privateCertKey = (RSACryptoServiceProvider)certificate.PrivateKey;
            return privateCertKey.Decrypt(encryptedData, false);
        }

        public static byte[] Encrypt(this X509Certificate2 certificate, byte[] data)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            var publicCertKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;
            return publicCertKey.Encrypt(data ?? Enumerable.Empty<byte>().ToArray(), false);
        }

        #endregion
    }
}