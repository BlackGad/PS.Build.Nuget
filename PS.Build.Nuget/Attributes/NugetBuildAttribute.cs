﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NuGet.Packaging;
using PS.Build.Extensions;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetBuildAttribute : Attribute
    {
        private readonly string _id;
        private readonly string _targetDirectory;

        #region Constructors

        public NugetBuildAttribute(string id, string targetDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            _id = id;
            _targetDirectory = targetDirectory;
        }

        #endregion

        #region Members

        private void PostBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            var package = provider.GetVaultPackage(_id);
            var targetDirectory = string.IsNullOrWhiteSpace(_targetDirectory)
                ? provider.GetService<IExplorer>().Directories[BuildDirectory.Target]
                : _targetDirectory;

            logger.Info("Building nuget package");
            logger.Info("ID: " + package.Metadata.Id);
            logger.Info("Version: " + package.Metadata.Version);
            logger.Info("Target: " + targetDirectory);
            try
            {
                var build = new PackageBuilder();
                build.Populate(package.Metadata);

                foreach (var file in package.Files)
                {
                    build.AddFiles(targetDirectory, file.Source, file.Destination, file.Exclude);
                }

                var finalPath = Path.Combine(targetDirectory, package.Metadata.Id + "." + package.Metadata.Version + ".nupkg");

                using (var stream = File.OpenWrite(finalPath))
                {
                    logger.Debug("Building...");
                    build.Save(stream);
                }
                logger.Info("Package successfuly created");
            }
            catch (Exception e)
            {
                logger.Error("Package build failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}