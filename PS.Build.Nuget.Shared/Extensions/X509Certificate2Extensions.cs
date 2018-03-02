using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using PS.Build.Extensions;

namespace PS.Build.Nuget.Extensions
{
    public static class X509Certificate2Extensions
    {
        #region Static members

        public static X509Certificate2 CreateAuthorityCertificate(string subjectName, out AsymmetricKeyParameter caPrivateKey)
        {
            const int keyStrength = 2048;

            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Issuer and Subject Name
            var subjectDN = new X509Name(subjectName);
            var issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(2);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            var issuerKeyPair = subjectKeyPair;
            var signatureFactory = new Asn1SignatureFactory("SHA512WithRSA", issuerKeyPair.Private, random);

            // selfsign certificate
            var certificate = certificateGenerator.Generate(signatureFactory);
            var x509 = new X509Certificate2(certificate.GetEncoded());

            caPrivateKey = issuerKeyPair.Private;

            return x509;
        }

        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName,
                                                                   string issuerName,
                                                                   AsymmetricKeyParameter issuerPrivKey = null)
        {
            const int keyStrength = 2048;

            // Generating Random Numbers
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // Subject Keys
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            var signatureFactory = new Asn1SignatureFactory("SHA512WithRSA", issuerPrivKey ?? subjectKeyPair.Private, random);

            // The Certificate Generator
            var certificateGenerator = new X509V3CertificateGenerator();
            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage,
                                              true,
                                              new ExtendedKeyUsage(new List<DerObjectIdentifier>
                                              {
                                                  new DerObjectIdentifier("1.3.6.1.5.5.7.3.1")
                                              }));

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Issuer and Subject Name
            var subjectDN = new X509Name(subjectName);
            var issuerDN = new X509Name(issuerName);
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(20);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // selfsign certificate
            var certificate = certificateGenerator.Generate(signatureFactory);

            // corresponding private key
            var info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);

            // merge into X509Certificate2
            var x509 = new X509Certificate2(certificate.GetEncoded());

            var seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());
            var rsa = RsaPrivateKeyStructure.GetInstance(seq);
            var rsaParams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus,
                rsa.PublicExponent,
                rsa.PrivateExponent,
                rsa.Prime1,
                rsa.Prime2,
                rsa.Exponent1,
                rsa.Exponent2,
                rsa.Coefficient);

            x509.PrivateKey = DotNetUtilities.ToRSA(rsaParams);
            return x509;
        }

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
            return publicCertKey.Encrypt(data.Enumerate().ToArray(), false);
        }

        #endregion
    }
}