using Force.Crc32;

namespace PS.Build.Nuget.Shared.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string CRC32(this byte[] data)
        {
            Crc32Algorithm.Compute(data).to
        }
    }
}
