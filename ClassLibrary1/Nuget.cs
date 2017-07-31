using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: NugetPackageDependenciesFromConfiguration]
[assembly: NugetPackageDependency("Yo")]
[assembly: NugetFilesFromTarget]
[assembly: NugetAuthor("me")]
[assembly: NugetBuild]

#endif