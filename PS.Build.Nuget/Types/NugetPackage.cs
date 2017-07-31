﻿using System;
using System.Collections.Generic;
using NuGet.Packaging;

namespace PS.Build.Nuget.Types
{
    internal class NugetPackage
    {
        #region Constructors

        public NugetPackage(ManifestMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            Metadata = metadata;

            IncludeFiles = new List<NugetPackageFiles>();
            ExcludeFiles = new List<NugetPackageFiles>();
            IncludeDependencies = new List<PackageReference>();
            ExcludeDependencies = new List<NugetPackageDependencyFilter>();
        }

        #endregion

        #region Properties

        public List<NugetPackageDependencyFilter> ExcludeDependencies { get; }

        public List<NugetPackageFiles> ExcludeFiles { get; }
        public List<PackageReference> IncludeDependencies { get; }

        public List<NugetPackageFiles> IncludeFiles { get; }

        public ManifestMetadata Metadata { get; }

        #endregion
    }
}