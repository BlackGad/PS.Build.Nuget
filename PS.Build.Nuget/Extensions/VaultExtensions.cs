using System;
using NuGet.Packaging;
using PS.Build.Nuget.Types;
using PS.Build.Services;

namespace PS.Build.Nuget.Extensions
{
    internal static class VaultExtensions
    {
        #region Static members

        internal static NugetPackage GetVaultPackage(this IDynamicVault vault, string id)
        {
            if (vault == null) throw new ArgumentNullException(nameof(vault));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            var vaultKey = "___Nuget package " + id;
            NugetPackage result;
            object vaultData;
            if (!vault.Query(vaultKey, out vaultData))
            {
                result = new NugetPackage(new ManifestMetadata());
                vault.Store(vaultKey, result);
            }
            result = vaultData as NugetPackage;
            if (result == null) throw new InvalidOperationException();
            return result;
        }

        #endregion
    }
}