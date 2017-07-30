using System;
using System.ComponentModel.DataAnnotations;
using PS.Build.Types;

namespace PS.Build.Nuget.Types
{
    class NugetExtensionMacroHandler : IMacroHandler
    {
        #region Properties

        public string ID => "PS.Build.Nuget";
        public int Order => 50;
        public string PackageFrameworkDirectory { get; set; }

        #endregion

        #region IMacroHandler Members

        public bool CanHandle(string key, string value, string formatting)
        {
            if (!string.Equals(key, "nuget", StringComparison.InvariantCultureIgnoreCase)) return false;
            switch (value?.ToLowerInvariant())
            {
                case "framework":
                    return true;
            }
            return false;
        }

        public HandledMacro Handle(string key, string value, string formatting)
        {
            switch (value?.ToLowerInvariant())
            {
                case "framework":
                    return new HandledMacro(PackageFrameworkDirectory);
            }
            return new HandledMacro(new ValidationResult($"Unexpected {value} value"));
        }

        #endregion
    }
}