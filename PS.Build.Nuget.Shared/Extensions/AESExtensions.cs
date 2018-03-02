using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PS.Build.Extensions;

namespace Cinegy.Serialization
{
    public static class AESExtensions
    {
        #region Constants

        private static readonly byte[] Salt = Encoding.ASCII.GetBytes("4a7185c888e52c62d");

        #endregion

        #region Static members

        /// <summary>
        ///     Decrypt the given data.  Assumes the data was encrypted using
        ///     EncryptAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="encryptedData">The data to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        public static byte[] DecryptAES(this byte[] encryptedData, string sharedSecret)
        {
            encryptedData = encryptedData.Enumerate().ToArray();
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException(nameof(sharedSecret));

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;
            byte[] result;
            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedSecret, Salt);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(encryptedData))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize/8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    //aesAlg.IV = msDecrypt.ToArray();
                    // Create a decrytor to perform the stream transform.
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var resultStream = new MemoryStream())
                        {
                            csDecrypt.CopyTo(resultStream);
                            result = resultStream.ToArray();
                        }
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                aesAlg?.Clear();
            }

            return result;
        }

        /// <summary>
        ///     Encrypt the given data using AES.  The data can be decrypted using
        ///     DecryptAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        public static byte[] EncryptAES(this byte[] data, string sharedSecret)
        {
            data = data.Enumerate().ToArray();
            if (string.IsNullOrEmpty(sharedSecret)) throw new ArgumentNullException(nameof(sharedSecret));

            RijndaelManaged aesAlg = null; // RijndaelManaged object used to encrypt the data.

            byte[] result;
            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedSecret, Salt);

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize/8);

                // Create a decryptor to perform the stream transform.
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                    }
                    result = msEncrypt.ToArray();
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                aesAlg?.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return result;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            var rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
                throw new SystemException("Stream did not contain properly formatted byte array");

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new SystemException("Did not read byte array properly");

            return buffer;
        }

        #endregion
    }
}