using System;

namespace PS.Build.Nuget.Attributes.Base
{
    public abstract class BaseNugetAttribute : Attribute
    {
        #region Properties

        public string ID { get; set; }

        #endregion
    }
}