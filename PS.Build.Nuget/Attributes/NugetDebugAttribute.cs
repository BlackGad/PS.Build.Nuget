using System;
using System.ComponentModel;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Nuget.Types;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetDebugAttribute : BaseNugetAttribute
    {
        private readonly string _configurationFilePath;

        #region Constructors

        public NugetDebugAttribute(string configurationFilePath = null, bool generateTemplateFile = true)
        {
            _configurationFilePath = configurationFilePath;
        }

        #endregion

        #region Members

        private void PostBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var package = provider.GetVaultPackage(ID);
            logger.Info("Debugging nuget package");
            try
            {
                logger.Info("Collecting package files");

                var config = new NugetDebugConfiguration
                {
                    Packages =
                    {
                        new NugetDebugPackage
                        {
                            ID = "Package1.ID",
                            Solutions =
                            {
                                new NugetDebugSolution
                                {
                                    Directory = "solution dir 1"
                                },
                                new NugetDebugSolution
                                {
                                    Directory = "solution dir 2"
                                }
                            }
                        },
                        new NugetDebugPackage
                        {
                            ID = "Package2.ID",
                            Solutions =
                            {
                                new NugetDebugSolution
                                {
                                    Directory = "solution dir 1"
                                }
                            }
                        }
                    }
                };
                config.SaveXml("e:\\temp\\sss.xml");
                foreach (var tuple in package.EnumerateFiles(logger))
                {
                }

                logger.Info("Package successfully created");
            }
            catch (Exception e)
            {
                logger.Error("Package build failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}