using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PS.Build.Nuget.Tasks
{
    public class ContentDecryptorTask : Task
    {
        #region Override members

        public override bool Execute()
        {
            var logger = new Services.Logger.Logger(Log);

            try
            {

            }
            catch (Exception e)
            {
                logger.Error($"Cannot decrypt nuget package content. Details: {e.GetBaseException().Message}");
            }

            return !Log.HasLoggedErrors;
        }

        #endregion
    }
}