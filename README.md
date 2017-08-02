# PS.Build adaptation to manage your project nuget packages
[![NuGet Version](https://img.shields.io/nuget/v/PS.Build.Nuget.svg?label=master+nuget)](https://www.nuget.org/packages?q=PS.Build.Nuget)
[![Build status](https://ci.appveyor.com/api/projects/status/ur6psdnxcljqnbxq?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build-nuget)
[![MyGet CI](https://img.shields.io/myget/ps-projects/vpre/ps.build.nuget.svg?label=CI+nuget)](https://www.myget.org/gallery/ps-projects)
[![Build status](https://ci.appveyor.com/api/projects/status/ok5hydixhinsm9rt?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build-nuget-oohdo)

Current adaptation allows you to create Nuget packages from your code. Also contains methods to easily debug local projects that has dependency from your nuget package.

# Getting started
* Create new [C# library](https://msdn.microsoft.com/en-us/library/f3cye135(v=vs.120).aspx) project. Name it **NugetAdaptationExample** (or skip this step if you already have any NET project that must produce nuget package)
* Add reference to [PS.Build.Nuget](https://www.nuget.org/packages/PS.Build.Nuget/) nuget package to project. Package will add ```nuget.package.txt``` file to root of your project with breaf API description
* (Optional) Select all ```Nuget.*```, ```PS.Build.Nuget```, ```PS.Build``` and, if you do not use in your runtime ```Newtonsoft.Json``` assembly references and set ```Copy local: false``` option (I like clear output folder)
* Copy ```Fast start``` section from ```nuget.package.txt``` to any cs file (```AssemblyInfo.cs``` or create ```nuget.package.cs``` compile file)
* Add reference to [PS.Build.Tasks](https://www.nuget.org/packages/PS.Build.Tasks/) nuget package to project. This step will enable build adaptation engine in this project
* Compile project and get errors:
```
1>D:\Projects\NugetAdaptationExample\packages\ps.build.tasks.1.18.0\build\PS.Build.Tasks.targets(26,3): error : Package build failed. Details: Authors is required.
1>D:\Projects\NugetAdaptationExample\packages\ps.build.tasks.1.18.0\build\PS.Build.Tasks.targets(26,3): error : Description is required.
```
* Finally setup [AssemblyDescriptionAttribute](https://msdn.microsoft.com/en-us/library/system.reflection.assemblydescriptionattribute(v=vs.110).aspx) and [AssemblyCompanyAttribute](https://msdn.microsoft.com/en-us/library/system.reflection.assemblycompanyattribute(v=vs.110).aspx)
* Compile project. Output must contain something like this:
```
1>  Adaptation: PS.Build.Nuget.Attributes.NugetBuildAttribute(null)
1>  Building nuget package
1>    Target directory: D:\Projects\NugetAdaptationExample\NugetAdaptationExample\bin\Debug\
1>    ID: NugetAdaptationExample
1>    Version: 1.0.0.0
1>    Copyright: Copyright ©  2017
1>    Description: Description
1>    Development dependency: False
1>    Require license acceptance: False
1>    Serviceable: False
1>    Title: NugetAdaptationExample
1>    Package framework references: None
1>    Package assembly references: 
1>      Assembly: NugetAdaptationExample.dll
1>        Framework: Any,Version=v0.0
1>    Package dependencies: None
1>    Package files: 
1>      Group: lib\net461
1>        + File: D:\Projects\NugetAdaptationExample\NugetAdaptationExample\bin\Debug\NugetAdaptationExample.dll
1>    Package successfully created
```
* Check ```D:\Projects\NugetAdaptationExample\NugetAdaptationExample\bin\Debug\NugetAdaptationExample.1.0.0.0.nupkg``` package 

# Documentation
Additional information could be found at project [wiki page](https://github.com/BlackGad/PS.Build.Nuget/wiki)
