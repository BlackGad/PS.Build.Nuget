namespace PS.Build.Nuget.Types
{
    internal class NugetPackageFiles
    {
        #region Constructors

        public NugetPackageFiles(string source, string destination)
        {
            Source = source;
            Destination = destination;
        }

        #endregion

        #region Properties

        public string Destination { get; }

        public string Source { get; }

        #endregion
    }
}