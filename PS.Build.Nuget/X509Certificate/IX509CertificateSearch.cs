using System.Security.Cryptography.X509Certificates;

namespace PS.Build.Nuget.X509Certificate
{
    public interface IX509CertificateSearch
    {
        #region Members

        X509Certificate2[] Search();

        #endregion
    }
}