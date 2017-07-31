using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: NugetPackageDependenciesFromConfiguration]
[assembly: NugetPackageDependency("PS.Build.N1aa")]
[assembly: NugetPackageDependenciesFilter(@"PS1*1*")]
//[assembly: NugetPackageDependency("Yo")]
[assembly: NugetFilesFromTarget(IncludePDB = true)]
[assembly: NugetFilesFilter(@"{dir.target}\*.pdb", @"lib\{nuget.framework}")]
[assembly: NugetAuthor("me")]
[assembly: NugetBuild]
 
#endif