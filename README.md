# PS.Build adaptation to manage your project nuget packages
[![NuGet Version](https://img.shields.io/nuget/v/PS.Build.Nuget.svg?label=master+nuget)](https://www.nuget.org/packages?q=PS.Build.Nuget)
[![Build status](https://ci.appveyor.com/api/projects/status/ur6psdnxcljqnbxq?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build-nuget)
[![MyGet CI](https://img.shields.io/myget/ps-projects/vpre/ps.build.nuget.svg?label=CI+nuget)](https://www.myget.org/gallery/ps-projects)
[![Build status](https://ci.appveyor.com/api/projects/status/ok5hydixhinsm9rt?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build-nuget-oohdo)

Current adaptation allows you to create Nuget packages from your code. Also contains methods to easily debug local projects that has dependency from your nuget package.

# Getting started
* Create new [C# library](https://msdn.microsoft.com/en-us/library/f3cye135(v=vs.120).aspx) project. Name it **NugetAdaptationExample** (or skip this step if you already have any NET project that must produce nuget package)
* Add reference to [PS.Build.Nuget](https://www.nuget.org/packages/PS.Build.Nuget/) nuget package to project. Package will add ```nuget.package.cs``` file to root of your project (If you need you can remove it and define adpatation attributes elsewhere, but for further instructions I will use this file)
* (Optional) Select all ```Nuget.*```, ```PS.Build.Nuget``` and, if you do not use in your runtime ```Newtonsoft.Json``` assembly references and set ```Copy local: false``` option (I like clear output folder)
* Nuget definition content:
```csharp
using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: NugetFilesFromTarget]
[assembly: NugetPackageDependenciesFromConfiguration]
//[assembly: NugetPackageDependenciesFilter("NuGet.*")]
//[assembly: NugetPackageDependenciesFilter("PS.Build.*")]
//[assembly: NugetPackageDependenciesFilter("Newtonsoft.Json")]
[assembly: NugetBuild]
[assembly: NugetDebugSubstitution]

#endif
```
Preprocessor directives here is [runtime isolation](https://github.com/BlackGad/PS.Build#preprocessor-directives-isolation).

```[assembly: NugetFilesFromTarget(<options>)]``` adapatation to parse project output and include it into nuget package

```[assembly: NugetPackageDependenciesFromConfiguration]``` adatation will parse your ```packages.config``` file and add all referenced nuget package references as Nuget package dependencies into your nuget package

```[assembly: NugetPackageDependenciesFilter("<Filter pattern>")]``` allows you to filter automatically included nuget dependencies.

```[assembly: NugetBuild("<target folder>")]``` instructs to build nuget package

```[assembly: NugetDebugSubstitution]``` instructions to substitute local projects that has dependency from this nuget package. By default adaptation will create blank ```NuGet.Debug.config``` file which contains destination solution folders where nuget package content must be updated.

* Add reference to [PS.Build.Tasks](https://www.nuget.org/packages/PS.Build.Tasks/) nuget package to project. This step will enable build adaptation engine in this project
* Compile project and get errors:
```
1>D:\Projects\NugetAdaptationExample\packages\ps.build.tasks.1.18.0\build\PS.Build.Tasks.targets(26,3): error : Package build failed. Details: Authors is required.
1>D:\Projects\NugetAdaptationExample\packages\ps.build.tasks.1.18.0\build\PS.Build.Tasks.targets(26,3): error : Description is required.
```



