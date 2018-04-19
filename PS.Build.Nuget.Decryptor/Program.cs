using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static X509CertificateSearch CertificatePackageConfigurationLoad(NugetEncryptionConfiguration encryptionConfiguration,
                                                                                string certificateFilePath)
        {
            var directSearch = !string.IsNullOrWhiteSpace(certificateFilePath);
            if (!directSearch)
            {
                Console.WriteLine("Trying to search default configuration...");
                certificateFilePath = Path.Combine(Environment.CurrentDirectory, "NuGet.Encryption.config");
            }

            try
            {
                var file = new FileInfo(certificateFilePath);
                do
                {
                    Console.WriteLine($"Trying to load configuration from {file.FullName}");
                    if (file.Exists)
                    {
                        Console.WriteLine("File exist. Trying to load certificate configuration...");
                        try
                        {
                            var certificateConfiguration = file.FullName.LoadXml<NugetCertificateConfiguration>();
                            Console.WriteLine("Configuration loaded. Trying to find required package configuration by id...");
                            certificateConfiguration.Packages = certificateConfiguration.Packages ?? new List<NugetCertificatePackage>();
                            var packageCertificate = certificateConfiguration
                                .Packages
                                .FirstOrDefault(p => string.Equals(p.ID,
                                                                   encryptionConfiguration.Metadata.ID,
                                                                   StringComparison.InvariantCultureIgnoreCase));

                            if (packageCertificate != null)
                            {
                                Console.WriteLine("Package configuration found.");
                                return packageCertificate.Search;
                            }
                            Console.WriteLine("Package configuration not found.");
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Configuration file is invalid.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("File not exist");
                    }

                    var directory = file.Directory;
                    if (directSearch || directory?.Parent == null) break;
                    file = new FileInfo(Path.Combine(directory.Parent.FullName, file.Name));
                } while (true);
            }
            catch (Exception)
            {
                //Nothing
            }

            return null;
        }

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
                Console.WriteLine("Hash file does not match neither encrypted nor normal.");
                return;
            }

            byte[] encryptedContent;
            if (file.Type == NugetEncryptionFileType.ManifestResource)
            {
                Console.WriteLine("Encrypted data stored inside manifest resource. Extracting...");
                using (var isolated = new Isolated<Unpacker>())
                {
                    encryptedContent = isolated.Value.Unpack(filePath);
                }
            }
            else
            {
                encryptedContent = File.ReadAllBytes(filePath);
            }

            Console.WriteLine("File is encrypted.");
            var decryptedContent = encryptedContent.DecryptAES(Encoding.UTF8.GetString(aesKey));
            var decryptedFilePath = filePath + ".decrypted";
            Console.WriteLine("File was successfully decrypted.");

            File.WriteAllBytes(decryptedFilePath, decryptedContent);
            Console.WriteLine($"Saving to intermediate path: {decryptedFilePath}");

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
            var writer = new AggregatedStandardOutputWriter();
            Console.SetOut(writer);

            if (string.Equals(args.FirstOrDefault(), "example", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Certificate search configuration example:");

                var template = new NugetCertificateConfiguration
                {
                    Packages = new List<NugetCertificatePackage>
                    {
                        new NugetCertificatePackage
                        {
                            ID = "nuget.package.id.1",
                            Search = new X509CertificateFileSearch
                            {
                                SourceFile = @"c:\certificate.pfx",
                                Password = "password"
                            }
                        },
                        new NugetCertificatePackage
                        {
                            ID = "nuget.package.id.2",
                            Search = new X509CertificateManifestResourceSearch
                            {
                                SourceAssembly = @"c:\assembly.dll",
                                ResourceName = "manifest.resource.name.pfx",
                                Password = "password"
                            }
                        },
                        new NugetCertificatePackage
                        {
                            ID = "nuget.package.id.3",
                            Search = new X509CertificateStorageSearch
                            {
                                StoreLocation = StoreLocation.LocalMachine,
                                StoreName = StoreName.My,
                                FindValue = "cert thumbprint",
                                FindType = X509FindType.FindByThumbprint
                            }
                        }
                    }
                };

                Console.WriteLine(template.SerializeXml());

                if (!args.Any(a => string.Equals(a, "-s", StringComparison.InvariantCultureIgnoreCase)))
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadLine();
                }

                return 0;
            }

            var configurationFilePath = ParseArgument("-config", args) ??
                                        Path.Combine(Environment.CurrentDirectory, "encryption.config");

            var certificateFilePath = ParseArgument("-certificate", args);

            try
            {
                Console.WriteLine($"Loading configuration from {configurationFilePath} file");
                var configuration = ConfigurationLoad(configurationFilePath);
                Console.WriteLine("Configuration loaded");

                var configurationFolder = Path.GetDirectoryName(configurationFilePath);
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

                var search = CertificatePackageConfigurationLoad(configuration, certificateFilePath);
                if (search == null)
                {
                    Console.WriteLine("Package certificate configuration not found. Using default configuration.");
                    search = new X509CertificateStorageSearch
                    {
                        StoreLocation = StoreLocation.CurrentUser,
                        StoreName = StoreName.My,
                        FindType = X509FindType.FindByThumbprint,
                        FindValue = configuration.Metadata.Certificate
                    };
                }

                var certificate = search.Search().FirstOrDefault();
                if (certificate == null) throw new ApplicationException("Could not find specified certificate");
                Console.WriteLine("Certificate found");

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

                try
                {
                    File.WriteAllText(configurationFilePath + ".fail", writer.GetText());
                }
                catch (Exception error)
                {
                    Console.WriteLine("WARN: Could not save success file marker near configuration. Details: " + error.GetBaseException().Message);
                }

                return 1;
            }
            finally
            {
                if (!args.Any(a => string.Equals(a, "-s", StringComparison.InvariantCultureIgnoreCase)))
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadLine();
                }
            }

            try
            {
                File.WriteAllText(configurationFilePath + ".pass", writer.GetText());
            }
            catch (Exception e)
            {
                Console.WriteLine("WARN: Could not save success file marker near configuration. Details: " + e.GetBaseException().Message);
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