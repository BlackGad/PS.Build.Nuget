using System.Security.Cryptography.X509Certificates;

namespace PS.Build.Nuget.X509Certificate
{
    public abstract class X509CertificateSearch
    {
        #region Members

        public abstract X509Certificate2[] Search();

        #endregion
    }
}