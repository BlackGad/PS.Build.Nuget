using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using PS.Build.Nuget.Shared.Sources;

namespace PS.Build.Nuget.Extensions
{
    public static class AssemblyExtensions
    {
        #region Static members

        /// <remarks>
        ///     https://github.com/BlackGad/PS.FileStructureAnalyzer
        /// </remarks>
        public static CompilationMode GetCompilationMode(this FileInfo info)
        {
            if (!info.Exists) throw new ArgumentException($"{info.FullName} does not exist");

            var intPtr = IntPtr.Zero;
            try
            {
                uint unmanagedBufferSize = 4096;
                intPtr = Marshal.AllocHGlobal((int)unmanagedBufferSize);

                using (var stream = File.Open(info.FullName, FileMode.Open, FileAccess.Read))
                {
                    var bytes = new byte[unmanagedBufferSize];
                    stream.Read(bytes, 0, bytes.Length);
                    Marshal.Copy(bytes, 0, intPtr, bytes.Length);
                }

                //Check DOS header magic number
                if (Marshal.ReadInt16(intPtr) != 0x5a4d) return CompilationMode.Invalid;

                // This will get the address for the WinNT header  
                var ntHeaderAddressOffset = Marshal.ReadInt32(intPtr + 60);

                // Check WinNT header signature
                var signature = Marshal.ReadInt32(intPtr + ntHeaderAddressOffset);
                if (signature != 0x4550) return CompilationMode.Invalid;

                //Determine file bitness by reading magic from IMAGE_OPTIONAL_HEADER
                var magic = Marshal.ReadInt16(intPtr + ntHeaderAddressOffset + 24);

                var result = CompilationMode.Invalid;
                uint clrHeaderSize;
                if (magic == 0x10b)
                {
                    clrHeaderSize = (uint)Marshal.ReadInt32(intPtr + ntHeaderAddressOffset + 24 + 208 + 4);
                    result |= CompilationMode.Bit32;
                }
                else if (magic == 0x20b)
                {
                    clrHeaderSize = (uint)Marshal.ReadInt32(intPtr + ntHeaderAddressOffset + 24 + 224 + 4);
                    result |= CompilationMode.Bit64;
                }
                else return CompilationMode.Invalid;

                result |= clrHeaderSize != 0
                    ? CompilationMode.CLR
                    : CompilationMode.Native;

                return result;
            }
            finally
            {
                if (intPtr != IntPtr.Zero) Marshal.FreeHGlobal(intPtr);
            }
        }

        public static string ResolveResourceName(this Assembly assembly, string resourceName)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrWhiteSpace(resourceName)) throw new ArgumentNullException(nameof(resourceName));
            var existingNames = assembly.GetManifestResourceNames();
            resourceName = resourceName.ResolveResourceNamespace();
            var name = existingNames.FirstOrDefault(n => n.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase));
            return name;
        }

        public static string ResolveResourceNamespace(this string resourceNamespace)
        {
            if (string.IsNullOrWhiteSpace(resourceNamespace)) throw new ArgumentNullException(nameof(resourceNamespace));
            return resourceNamespace.Replace("/", ".").Replace("\\", ".");
        }

        #endregion
    }
}