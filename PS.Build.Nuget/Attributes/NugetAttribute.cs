using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Versioning;
using PS.Build.Extensions;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetAttribute : Attribute
    {
        private readonly string _id;

        #region Constructors

        public NugetAttribute(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Invalid id");
            _id = id;
        }

        #endregion

        #region Properties

        public string Copyright { get; set; }
        public string Description { get; set; }
        public bool? DevelopmentDependency { get; set; }
        public string IconUrl { get; set; }
        public string Language { get; set; }
        public string LicenseUrl { get; set; }
        public string MinClientVersion { get; set; }
        public string ProjectUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public bool? RequireLicenseAcceptance { get; set; }
        public bool? Serviceable { get; set; }
        public string Summary { get; set; }
        public string Tags { get; set; }

        public string Title { get; set; }
        public string Version { get; set; }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            try
            {
                logger.Debug("Defining nuget package properties");
                var package = provider.GetVaultPackage(_id);

                if (Copyright != null) package.Metadata.Copyright = Copyright;
                if (Description != null) package.Metadata.Description = Description;
                if (DevelopmentDependency != null) package.Metadata.DevelopmentDependency = DevelopmentDependency.Value;
                if (IconUrl != null) package.Metadata.SetIconUrl(IconUrl);
                if (Language != null) package.Metadata.Language = Language;
                if (LicenseUrl != null) package.Metadata.SetLicenseUrl(LicenseUrl);
                if (MinClientVersion != null) package.Metadata.MinClientVersionString = MinClientVersion;
                if (ProjectUrl != null) package.Metadata.SetProjectUrl(ProjectUrl);
                if (ReleaseNotes != null) package.Metadata.ReleaseNotes = ReleaseNotes;
                if (RequireLicenseAcceptance != null) package.Metadata.RequireLicenseAcceptance = RequireLicenseAcceptance.Value;
                if (Serviceable != null) package.Metadata.Serviceable = Serviceable.Value;
                if (Summary != null) package.Metadata.Summary = Summary;
                if (Tags != null) package.Metadata.Tags = Tags;
                if (Title != null) package.Metadata.Title = Title;
                if (Version != null) package.Metadata.Version = NuGetVersion.Parse(Version);
            }
            catch (Exception e)
            {
                logger.Error("Package definition failed. Details: " + e.GetBaseException().Message);
            }
        }

        #endregion
    }
}