using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PS.Build.Nuget.Shared.Extensions
{
    public static class ByteArrayExtensions
    {
        #region Static members

        public static string ComputeHashMD5(this string filename)
        {
            if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename)) return null;

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return md5.ComputeHash(stream).ToHexString();
                }
            }
        }

        public static string ComputeHashMD5(this byte[] array)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(array))
                {
                    return md5.ComputeHash(stream).ToHexString();
                }
            }
        }

        public static byte[] FromHexString(this string hex)
        {
            if (hex == null) return Enumerable.Empty<byte>().ToArray();

            var numberChars = hex.Length;
            var bytes = new byte[numberChars/2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] ReadStream(this Stream input)
        {
            var buffer = new byte[16*1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static string ToHexString(this byte[] ba)
        {
            var strhex = BitConverter.ToString(ba);
            return strhex.Replace("-", "");
        }

        #endregion
    }
}