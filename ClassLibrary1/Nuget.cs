using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: NugetPackageDependenciesFromConfiguration]
[assembly: NugetPackageDependenciesFilter(@"PS.Build.*")]
//[assembly: NugetFrameworkReference(@"Microsoft.CSharp")]
//[assembly: NugetPackageAssemblyReference(@"{prop.TargetName}", "net452")]
[assembly: NugetFilesFromTarget]
[assembly: NugetFiles(@"{dir.target}\*.config", @"lib\{nuget.framework}")]
//[assembly: NugetFilesFilter(@"{dir.target}\*.pdb", @"lib\{nuget.framework}")]
[assembly: NugetBuild]
 
#endif