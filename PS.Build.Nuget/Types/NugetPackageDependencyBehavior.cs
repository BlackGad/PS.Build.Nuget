using System;

namespace PS.Build.Nuget.Types
{
    [Serializable]
    public enum NugetPackageDependencyBehavior
    {
        None,
        Major,
        Minor,
        Patch,
        Revision
    }
}