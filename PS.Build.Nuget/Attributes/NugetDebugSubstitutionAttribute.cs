using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Shared.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;
using PS.Build.Types;
using NugetPackage = PS.Build.Nuget.Types.NugetPackage;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetDebugSubstitutionAttribute : BaseNugetAttribute
    {
        private readonly string _configurationFilePath;
        private readonly bool _generateTemplateFile;

        #region Constructors

        public NugetDebugSubstitutionAttribute(string configurationFilePath = null, bool generateTemplateFile = true)
        {
            _configurationFilePath = configurationFilePath;
            _generateTemplateFile = generateTemplateFile;
        }

        #endregion

        #region Members

        private NugetDebugConfiguration HandleConfiguration(string configurationFilePath, ILogger logger, NugetPackage package)
        {
            if (!File.Exists(configurationFilePath))
            {
                logger.Debug("Configuration file does not exist");
                return null;
            }
            NugetDebugConfiguration configuration;
            try
            {
                logger.Debug("Loading configuration file");
                configuration = configurationFilePath.LoadXml<NugetDebugConfiguration>();
            }
            catch (Exception e)
            {
                logger.Error($"Could not load debug configuration in '{configurationFilePath}'. " +
                             $"Details: {e.GetBaseException().Message}");
                return null;
            }

            var existingPackageConfiguration = configuration.Packages
                                              .FirstOrDefault(p => string.Equals(p.ID,
                                                                                 package.Metadata.Id,
                                                                                 StringComparison.InvariantCultureIgnoreCase));
            if (existingPackageConfiguration?.Solutions.Any(s => s.Directory != string.Empty) != true)
            {
                logger.Debug("Configuration file does not contains instructions for current package");
                return configuration;
            }

            logger.Info("Collecting package files");
            var files = package.EnumerateFiles(logger).ToList();
            if (!files.Any()) return configuration;

            foreach (var solution in existingPackageConfiguration.Solutions)
            {
                if (string.IsNullOrWhiteSpace(solution.Directory)) continue;
                logger.Info($"Solution: {solution.Directory}");
                try
                {
                    var nugetExplorer = new NugetExplorer(solution.Directory);
                    var nugetPackage = nugetExplorer.FindPackage(package.Metadata.Id);
                    if (nugetPackage == null) logger.Warn($"Solution {solution.Directory} have no {package.Metadata.Id} package references");
                    else
                    {
                        using (logger.IndentMessages())
                        {
                            foreach (var file in files)
                            {
                                var destination = Path.Combine(nugetPackage.Folder, file.Destination);
                                var filename = Path.GetFileName(file.Source) ?? string.Empty;
                                if (!destination.EndsWith(filename, StringComparison.InvariantCultureIgnoreCase))
                                    destination = Path.Combine(destination, filename);

                                logger.Info($"Copying {file.Source} to {destination}");
                                File.Copy(file.Source, destination, true);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Could not handle debug nuget package substitution for {solution.Directory}. Details: {e.GetBaseException().Message}");
                }
            }

            return configuration;
        }

        private NugetDebugConfiguration ManageTemplateRecord(NugetDebugConfiguration config, NugetPackage package)
        {
            config = config ?? new NugetDebugConfiguration();
            var packageConfiguration = config.Packages.FirstOrDefault(p => string.Equals(p.ID, package.Metadata.Id, StringComparison.InvariantCultureIgnoreCase));

            if (packageConfiguration == null)
            {
                packageConfiguration = new NugetDebugPackage
                {
                    ID = package.Metadata.Id
                };

                config.Packages.Add(packageConfiguration);
            }
            if (!packageConfiguration.Solutions.Any())
            {
                packageConfiguration.Solutions.Add(new NugetDebugSolution
                {
                    Directory = string.Empty
                });
            }

            return config;
        }

        private void PostBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var explorer = provider.GetService<IExplorer>();
            var package = provider.GetVaultPackage(ID);
            logger.Info($"Substituting '{package.Metadata.Id}' nuget package dependant solutions");
            try
            {
                var configurationFilePath = string.IsNullOrWhiteSpace(_configurationFilePath)
                    ? Path.Combine(explorer.Directories[BuildDirectory.Solution], "NuGet.Debug.config")
                    : _configurationFilePath;

                configurationFilePath = provider.GetService<IMacroResolver>().Resolve(configurationFilePath);
                using (logger.IndentMessages())
                {
                    logger.Info($"Configuration: {configurationFilePath}");

                    var configuration = HandleConfiguration(configurationFilePath, logger, package);

                    if (_generateTemplateFile && !logger.HasErrors)
                    {
                        configuration = ManageTemplateRecord(configuration, package);
                        configuration.SaveXml(configurationFilePath);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Nuget package {package.Metadata.Id} debug substitution failed. Details: " + e.GetBaseException().Message);
            }

            if (logger.HasErrors) logger.Warn($"Nuget package {package.Metadata.Id} debug substitutions processed with errors");
            else logger.Info("Debug Nuget package substitutions successfully processed");
        }

        #endregion
    }
}