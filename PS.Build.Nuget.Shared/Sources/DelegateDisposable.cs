using System;

namespace PS.Build.Nuget.Shared.Sources
{
    internal class DelegateDisposable : IDisposable
    {
        private readonly Action _action;

        #region Constructors

        public DelegateDisposable(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _action();
        }

        #endregion
    }
}