using System;

namespace PS.Build.Nuget.Shared.Sources
{
    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        private AppDomain _domain;

        #region Constructors

        public Isolated()
        {
            _domain = AppDomain.CreateDomain("Isolated:" + Guid.NewGuid(),
                                             AppDomain.CurrentDomain.Evidence,
                                             AppDomain.CurrentDomain.SetupInformation);

            var type = typeof(T);

            Value = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        #endregion

        #region Properties

        public T Value { get; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_domain == null) return;

            AppDomain.Unload(_domain);
            _domain = null;
        }

        #endregion
    }
}