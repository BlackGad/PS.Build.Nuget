using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PS.Build.Extensions;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Shared.Extensions;
using PS.Build.Nuget.Shared.Sources;

namespace PS.Build.Nuget.Types
{
    class EncryptionSession
    {
        #region Constructors

        public EncryptionSession(string packageId, string temporaryDirectory, X509Certificate2 certificate)
        {
            EncryptionKey = Guid.NewGuid().ToString("N");
            EncryptedFilesDirectory = Path.Combine(temporaryDirectory, "__encrypted");
            EncryptedFilesDirectory.EnsureDirectoryExist();

            Configuration = new NugetEncryptionConfiguration
            {
                Metadata = new NugetEncryptionMetadata
                {
                    ID = packageId,
                    Certificate = certificate?.Thumbprint,
                    Key = certificate?.Encrypt(Encoding.UTF8.GetBytes(EncryptionKey)).ToHexString()
                }
            };
        }

        #endregion

        #region Properties

        public NugetEncryptionConfiguration Configuration { get; }

        public string EncryptedFilesDirectory { get; }
        public string EncryptionKey { get; }

        #endregion

        #region Members

        /// <summary>
        ///     Encrypt file
        /// </summary>
        /// <param name="filePath">Source file path</param>
        /// <param name="encryptedFilePath">Encrypted file path</param>
        public NugetEncryptionFile EncryptFile(string filePath, string encryptedFilePath)
        {
            var encryptedFileContent = File.ReadAllBytes(filePath).EncryptAES(EncryptionKey);

            var encryptionFile = new NugetEncryptionFile
            {
                EncryptedHash = encryptedFileContent.ComputeHashMD5(),
                OriginalHash = filePath.ComputeHashMD5(),
                Type = NugetEncryptionFileType.Direct
            };

            var encryptedFileDirectory = Path.GetDirectoryName(encryptedFilePath);
            encryptedFileDirectory.EnsureDirectoryExist();

            var compilationMode = filePath.GetCompilationMode();
            if (compilationMode == CompilationMode.CLR)
            {
                try
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(filePath);
                    var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
                    var compilation = CSharpCompilation.Create(assemblyName, options: compilationOptions);
                    var manifestResource = new ResourceDescription("encrypted", () => new MemoryStream(encryptedFileContent), true);
                    var result = compilation.Emit(encryptedFilePath, manifestResources: new[] { manifestResource });
                    if (!result.Success) throw new InvalidOperationException();
                    encryptionFile.Type = NugetEncryptionFileType.ManifestResource;
                    encryptionFile.EncryptedHash = encryptedFilePath.ComputeHashMD5();
                }
                catch (Exception)
                {
                    //Nothing
                }
            }

            if (encryptionFile.Type == NugetEncryptionFileType.Direct)
            {
                encryptionFile.Type = NugetEncryptionFileType.Direct;
                File.WriteAllBytes(encryptedFilePath, encryptedFileContent);
            }

            Configuration.Files.Add(encryptionFile);
            return encryptionFile;
        }

        public string SaveConfiguration()
        {
            var encryptionConfigurationFilePath = Path.Combine(EncryptedFilesDirectory, "encryption.config");
            Configuration.SaveXml(encryptionConfigurationFilePath);
            return encryptionConfigurationFilePath;
        }

        #endregion
    }
}