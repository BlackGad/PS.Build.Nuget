namespace PS.Build.Nuget.Types
{
    class NugetPackageFile
    {
        #region Properties

        public bool Encrypt { get; set; }
        public string Destination { get; set; }
        public string Source { get; set; }

        #endregion
    }
}