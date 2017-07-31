using NuGet.Frameworks;

namespace PS.Build.Nuget.Types
{
    internal class NugetPackageDependencyFilter
    {
        #region Constructors

        public NugetPackageDependencyFilter(string mask, NuGetFramework framework)
        {
            Mask = mask;
            Framework = framework;
        }

        #endregion

        #region Properties

        public NuGetFramework Framework { get; }

        public string Mask { get; }

        #endregion
    }
}