namespace PS.Build.Nuget.Types
{
    internal class NugetPackageFiles
    {
        #region Constructors

        public NugetPackageFiles(string source, string destination, bool encrypt = false)
        {
            Source = source;
            Destination = destination;
            Encrypt = encrypt;
        }

        #endregion

        #region Properties

        public string Destination { get; }

        public bool Encrypt { get; set; }

        public string Source { get; }

        #endregion
    }
}