using System;
using System.IO;
using System.Security.Cryptography;

namespace PS.Build.Nuget.Shared.Extensions
{
    public static class ByteArrayExtensions
    {
        #region Static members

        public static string ComputeHashMD5(this string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return md5.ComputeHash(stream).ToHexString();
                }
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