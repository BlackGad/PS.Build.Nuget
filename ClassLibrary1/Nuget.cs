using ClassLibrary1;
using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: Nuget(Const.ID, Version = "1.0.0.0")]
[assembly: NugetAuthors(Const.ID, "Me")]
[assembly: NugetFile(Const.ID, @"{dir.target}{prop.targetName}.pdb", @"lib\{nuget.framework}")]
[assembly: NugetBuild(Const.ID)]

namespace ClassLibrary1
{
    class Const
    {
        #region Constants

        public const string ID = "test";

        #endregion
    }
}

#endif