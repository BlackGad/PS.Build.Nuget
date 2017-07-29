using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Files = new List<NugetPackageFiles>();
        }

        #endregion

        #region Properties

        public List<NugetPackageFiles> Files { get; }
        public ManifestMetadata Metadata { get; }

        #endregion
    }
}
