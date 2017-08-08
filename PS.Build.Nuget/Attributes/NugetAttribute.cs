using System;
using System.ComponentModel;
using NuGet.Versioning;
using PS.Build.Extensions;
using PS.Build.Nuget.Attributes.Base;
using PS.Build.Nuget.Extensions;
using PS.Build.Services;

namespace PS.Build.Nuget.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Designer("PS.Build.Adaptation")]
    public sealed class NugetAttribute : BaseNugetAttribute
    {
        private bool? _developmentDependency;
        private bool? _requireLicenseAcceptance;
        private bool? _serviceable;

        #region Properties

        /// <summary>
        ///     (1.5+) Copyright details for the package. Default value mapped from AssemblyCopyright attribute.
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        ///     A long description of the package for UI display. Default value mapped from AssemblyDescription attribute.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     (2.8+) A Boolean value specifying whether the package is be marked as a development-only-dependency, which prevents
        ///     the package from being included as a dependency in other packages.
        /// </summary>
        public bool DevelopmentDependency
        {
            get { return _developmentDependency ?? false; }
            set { _developmentDependency = value; }
        }

        /// <summary>
        ///     A URL for a 64x64 image with transparency background to use as the icon for the package in UI display. Be sure this
        ///     element contains the direct image URL and not the URL of a web page containing the image.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        ///     The locale ID for the package. See Creating localized packages. Default value mapped from AssemblyCulture
        ///     attribute.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        ///     A URL for the package's license, often shown in UI displays as well as nuget.org.
        /// </summary>
        public string LicenseUrl { get; set; }

        /// <summary>
        ///     (2.5+) Specifies the minimum version of the NuGet client that can install this package, enforced by nuget.exe and
        ///     the Visual Studio Package Manager. This is used whenever the package depends on specific features of the .nuspec
        ///     file that were added in a particular version of the NuGet client. For example, a package using the
        ///     developmentDependency attribute should specify "2.8" for minClientVersion. Similarly, a package using the
        ///     contentFiles element (see the next section) should set minClientVersion to "3.3". Note also that because NuGet
        ///     clients prior to 2.5 do not recognize this flag, they always refuse to install the package no matter what
        ///     minClientVersion contains.
        /// </summary>
        public string MinClientVersion { get; set; }

        /// <summary>
        ///     A URL for the package's home page, often shown in UI displays as well as nuget.org.
        /// </summary>
        public string ProjectUrl { get; set; }

        /// <summary>
        ///     (1.5+) A description of the changes made in this release of the package, often used in UI like the Updates tab of
        ///     the Visual Studio Package Manager in place of the package description.
        /// </summary>
        public string ReleaseNotes { get; set; }

        /// <summary>
        ///     A Boolean value specifying whether the client must prompt the consumer to accept the package license before
        ///     installing the package.
        /// </summary>
        public bool RequireLicenseAcceptance
        {
            get { return _requireLicenseAcceptance ?? false; }
            set { _requireLicenseAcceptance = value; }
        }

        /// <summary>
        ///     (3.3+)For internal NuGet use only.
        /// </summary>
        public bool Serviceable
        {
            get { return _serviceable ?? false; }
            set { _serviceable = value; }
        }

        /// <summary>
        ///     A short description of the package for UI display. If omitted, a truncated version of description is used.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        ///     A space-delimited list of tags and keywords that describe the package and aid discoverability of packages through
        ///     search and filtering.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        ///     A human-friendly title of the package, typically used in UI displays as on nuget.org and the Package Manager in
        ///     Visual Studio. If not specified, the package ID is used.
        ///     Default value mapped from AssemblyTitle attribute.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The version of the package, following the major.minor.patch pattern. Version numbers may include a pre-release
        ///     suffix as described in Prerelease Packages.
        /// </summary>
        public string Version { get; set; }

        #endregion

        #region Members

        private void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            try
            {
                logger.Debug("Defining nuget package properties");
                var package = provider.GetVaultPackage(ID);

                if (Copyright != null) package.Metadata.Copyright = Copyright;
                if (Description != null) package.Metadata.Description = Description;
                if (_developmentDependency != null) package.Metadata.DevelopmentDependency = _developmentDependency.Value;
                if (IconUrl != null) package.Metadata.SetIconUrl(IconUrl);
                if (Language != null) package.Metadata.Language = Language;
                if (LicenseUrl != null) package.Metadata.SetLicenseUrl(LicenseUrl);
                if (MinClientVersion != null) package.Metadata.MinClientVersionString = MinClientVersion;
                if (ProjectUrl != null) package.Metadata.SetProjectUrl(ProjectUrl);
                if (ReleaseNotes != null) package.Metadata.ReleaseNotes = ReleaseNotes;
                if (_requireLicenseAcceptance != null) package.Metadata.RequireLicenseAcceptance = _requireLicenseAcceptance.Value;
                if (_serviceable != null) package.Metadata.Serviceable = _serviceable.Value;
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