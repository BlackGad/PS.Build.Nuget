using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PS.Build.Nuget.Shared.Sources;

namespace PS.Build.Nuget.Extensions
{
    public static class AssemblyExtensions
    {
        #region Static members

        /// <remarks>
        ///     https://msdn.microsoft.com/en-us/library/c91d4yzb.aspx?f=255&MSPPError=-2147217396
        /// </remarks>
        public static CompilationMode GetCompilationMode(this string filePath)
        {
            var data = new byte[4096];
            var file = new FileInfo(filePath);
            if (!file.Exists) return CompilationMode.Invalid;

            using (var stream = file.Open(FileMode.Open, FileAccess.Read))
            {
                stream.Read(data, 0, data.Length);
            }

            // Verify this is a executable/dll  
            if ((data[1] << 8 | data[0]) != 0x5a4d)
                return CompilationMode.Invalid;

            // This will get the address for the WinNT header  
            var iWinNTHdr = data[63] << 24 | data[62] << 16 | data[61] << 8 | data[60];

            // Verify this is an NT address  
            if ((data[iWinNTHdr + 3] << 24 | data[iWinNTHdr + 2] << 16 | data[iWinNTHdr + 1] << 8 | data[iWinNTHdr]) != 0x00004550)
                return CompilationMode.Invalid;

            var iLightningAddr = iWinNTHdr + 24 + 208;
            var iSum = 0;
            var iTop = iLightningAddr + 8;

            for (var i = iLightningAddr; i < iTop; ++i)
                iSum |= data[i];

            return iSum == 0
                ? CompilationMode.Native
                : CompilationMode.CLR;
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