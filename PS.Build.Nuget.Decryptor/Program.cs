﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Shared.Extensions;
using PS.Build.Nuget.Shared.Sources;
using PS.Build.Nuget.X509Certificate;

namespace PS.Build.Nuget.Decryptor
{
    public static class Program
    {
        #region Static members

        public static IEnumerable<NugetEncryptionFile> ConfigurationGetEncryptedFiles(string configurationFolder,
                                                                                      NugetEncryptionConfiguration configuration)
        {
            var files = configuration.Files ?? Enumerable.Empty<NugetEncryptionFile>();
            foreach (var f in files)
            {
                var filePath = Path.Combine(configurationFolder, f.Origin);
                Console.WriteLine($"  Checking {filePath}");
                Console.WriteLine($"    Expected encrypted hash: {f.EncryptedHash}");
                Console.WriteLine($"    Expected decrypted hash: {f.OriginalHash}");

                var realHash = filePath.ComputeHashMD5();
                Console.WriteLine($"    Current file hash: {realHash ?? "<null>"}");

                if (string.Equals(realHash, f.EncryptedHash, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("  File is encrypted.");
                    yield return f;
                }
                else
                {
                    Console.WriteLine("  File is not encrypted or missed. Will be skipped.");
                }
            }
        }

        public static NugetEncryptionConfiguration ConfigurationLoad(string configurationLocation)
        {
            if (!File.Exists(configurationLocation))
            {
                throw new ApplicationException("Cannot find encryption configuration file");
            }

            NugetEncryptionConfiguration configuration;
            try
            {
                configuration = configurationLocation.LoadXml<NugetEncryptionConfiguration>();
                if (configuration == null) throw new InvalidCastException();
            }
            catch (Exception)
            {
                throw new ApplicationException($"Could not parse {configurationLocation} file");
            }
            return configuration;
        }

        public static void DecryptFile(string configurationFolder, NugetEncryptionFile file, byte[] aesKey)
        {
            var filePath = Path.Combine(configurationFolder, file.Origin);
            Console.WriteLine($"Checking {filePath} file");
            if (!File.Exists(filePath)) throw new FileNotFoundException(string.Empty, filePath);

            var realHash = filePath.ComputeHashMD5();
            Console.WriteLine("Computing file hash...");
            Console.WriteLine($"  File hash: {realHash ?? "<null>"}");
            Console.WriteLine($"  Expected encrypted hash: {file.EncryptedHash}");
            Console.WriteLine($"  Expected decrypted hash: {file.OriginalHash}");

            if (string.Equals(realHash, file.OriginalHash, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("File is not encrypted.");
                return;
            }

            if (!string.Equals(realHash, file.EncryptedHash, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("File is corrupted");
            }

            byte[] encryptedContent;
            if (file.Type == NugetEncryptionFileType.ManifestResource)
            {
                Console.WriteLine("Encrypted data stored inside manifest resource. Extracting...");
                var fakeAssembly = Assembly.LoadFrom(filePath);
                var resourceName = fakeAssembly.GetManifestResourceNames().FirstOrDefault();
                if (string.IsNullOrWhiteSpace(resourceName)) throw new ArgumentException("Fake assembly does not contains any resources");
                using (var stream = fakeAssembly.GetManifestResourceStream(resourceName))
                {
                    encryptedContent = stream.ReadStream();
                }
            }
            else
            {
                encryptedContent = File.ReadAllBytes(filePath);
            }

            Console.WriteLine("File is encrypted.");
            
            var decryptedContent = encryptedContent.DecryptAES(Encoding.UTF8.GetString(aesKey));
            var decryptedFilePath = filePath + ".decrypted";

            File.WriteAllBytes(decryptedFilePath, decryptedContent);

            var decryptedHash = decryptedFilePath.ComputeHashMD5();
            if (!string.Equals(decryptedHash, file.OriginalHash, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException("File was decrypted successfully but final hash differs from expected");
            }

            Console.WriteLine("Swapping files...");
            File.Move(filePath, filePath + ".encrypted");
            File.Move(decryptedFilePath, filePath);
            Console.WriteLine($"{filePath} processed");
        }

        public static int Main(string[] args)
        {
            var configurationLocation = ParseArgument("-config", args) ??
                                        Path.Combine(Environment.CurrentDirectory, "encryption.config");

            var certificateFileLocation = ParseArgument("-cFile", args);

            try
            {
                Console.WriteLine($"Loading configuration from {configurationLocation} file");
                var configuration = ConfigurationLoad(configurationLocation);
                Console.WriteLine("Configuration loaded");

                var configurationFolder = Path.GetDirectoryName(configurationLocation);
                if (configurationFolder == null) throw new InvalidOperationException();

                var files = configuration.Files ?? Enumerable.Empty<NugetEncryptionFile>().ToList();
                if (!files.Any())
                {
                    Console.WriteLine("There are no available encrypted files");
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(configuration.Metadata?.Certificate))
                    throw new ArgumentException("Configuration metadata certificate thumbprint is corrupted");

                if (string.IsNullOrWhiteSpace(configuration.Metadata?.Key))
                    throw new ArgumentException("Configuration metadata encryption key corrupted");

                IX509CertificateSearch search;
                if (certificateFileLocation != null)
                {
                    if (!File.Exists(certificateFileLocation)) throw new ArgumentException($"Could not find {certificateFileLocation} certificate");
                    var password = ParseArgument("-cFilePassword", args);

                    Console.WriteLine($"Loading certificate from {certificateFileLocation} file");
                    if (!string.IsNullOrEmpty(password)) Console.WriteLine("With password");

                    search = new X509CertificateFileSearch
                    {
                        SourceFile = certificateFileLocation,
                        Password = password
                    };
                }
                else
                {
                    var certificateStoreLocation = ParseArgument("-cLocation", args);

                    var storeLocation = StoreLocation.CurrentUser;
                    if (certificateStoreLocation != null && !Enum.TryParse(certificateStoreLocation, true, out storeLocation))
                    {
                        throw new ArgumentException($"{certificateStoreLocation} certificate store location is not recognizaed");
                    }

                    var certificateStoreName = ParseArgument("-cName", args);

                    var storeName = StoreName.My;
                    if (certificateStoreName != null && !Enum.TryParse(certificateStoreName, true, out storeName))
                    {
                        throw new ArgumentException($"{certificateStoreName} certificate store name is not recognizaed");
                    }

                    Console.WriteLine($"Loading certificate from {storeLocation}.{storeName} store");

                    search = new X509CertificateStorageSearch
                    {
                        StoreLocation = storeLocation,
                        StoreName = storeName,
                        FindType = X509FindType.FindByThumbprint,
                        FindValue = configuration.Metadata?.Certificate
                    };
                }

                var certificate = search.Search().FirstOrDefault();
                Console.WriteLine("Certificate found");
                if (certificate == null) throw new ApplicationException("Could not find specified certificate");

                if (!string.Equals(certificate.Thumbprint, configuration.Metadata.Certificate, StringComparison.InvariantCultureIgnoreCase))
                    throw new ApplicationException("Loaded certificate thumbprint differs from required by configuration");

                Console.WriteLine("Decrypting AES key...");
                var encryptedAESKey = configuration.Metadata.Key.FromHexString();
                var decryptedAESKey = certificate.Decrypt(encryptedAESKey);
                Console.WriteLine("AES key successfully decrypted.");
                Console.WriteLine("Decrypting files:");
                foreach (var encryptedFile in files)
                {
                    try
                    {
                        DecryptFile(configurationFolder, encryptedFile, decryptedAESKey);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("WARN: " + e.GetBaseException().Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.GetBaseException().Message);
                return 1;
            }
            finally
            {
                Console.WriteLine("Press any key to exit");
                if (!args.Any(a => string.Equals(a, "-s", StringComparison.InvariantCultureIgnoreCase))) Console.ReadLine();
            }

            return 0;
        }

        private static string ParseArgument(string argument, string[] arguments)
        {
            var result = arguments.SkipWhile(a => !string.Equals(a, argument, StringComparison.InvariantCultureIgnoreCase))
                                  .Take(1)
                                  .FirstOrDefault();
            if (result?.StartsWith("-") == true) return null;
            return result;
        }

        #endregion
    }
}