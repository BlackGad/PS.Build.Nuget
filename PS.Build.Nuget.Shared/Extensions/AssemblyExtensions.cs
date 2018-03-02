using System;
using System.Linq;
using System.Reflection;

namespace PS.Build.Nuget.Extensions
{
    public static class AssemblyExtensions
    {
        #region Static members

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