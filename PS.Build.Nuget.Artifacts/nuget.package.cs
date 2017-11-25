using PS.Build.Nuget.Attributes;

[assembly: Nuget(Title = "PS.Build Nuget Adaptation", ID = "PS.Build.Nuget")]
[assembly: Nuget(ProjectUrl = "https://github.com/BlackGad/PS.Build.Nuget", ID = "PS.Build.Nuget")]
[assembly: Nuget(LicenseUrl = "https://github.com/BlackGad/PS.Build.Nuget/blob/master/LICENSE", ID = "PS.Build.Nuget")]
[assembly: Nuget(Tags = "PS.Build Nuget", ID = "PS.Build.Nuget")]
[assembly: NugetFiles(@"{dir.solution}\PS.Build.Nuget\bin\{prop.configuration}\PS.Build.Nuget.dll", @"lib\{nuget.framework}", ID = "PS.Build.Nuget")]
[assembly: NugetFiles(@"{dir.solution}\PS.Build.Nuget\bin\{prop.configuration}\Nuget.*.dll", @"lib\{nuget.framework}", ID = "PS.Build.Nuget")]
[assembly: NugetFiles(@"{dir.project}\Content\**.\*.*", @"content", ID = "PS.Build.Nuget")]
[assembly: NugetPackageAssemblyReference(@"PS.Build.Nuget.dll", ID = "PS.Build.Nuget")]
[assembly: NugetBuild(@"{dir.solution}_Artifacts\{prop.configuration}.{prop.platform}", ID = "PS.Build.Nuget")]
[assembly: NugetDebugSubstitution(ID = "PS.Build.Nuget")]