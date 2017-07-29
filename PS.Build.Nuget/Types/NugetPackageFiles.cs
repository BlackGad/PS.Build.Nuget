namespace PS.Build.Nuget.Types
{
    internal class NugetPackageFiles
    {
        #region Constructors

        public NugetPackageFiles(string source, string destination, string exclude = null)
        {
            Source = source;
            Destination = destination;
            Exclude = exclude;
        }

        #endregion

        #region Properties

        public string Exclude { get; }
        public string Source { get; }
        public string Destination { get; }

        #endregion
    }
}