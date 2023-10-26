using System;

namespace AnotherECS.Core
{
    public unsafe abstract class BaseState : IState, IDisposable
    {
        public bool IsDisposed { get; private set; }

        ~BaseState()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                OnDispose();
                GC.SuppressFinalize(this);
            }
        }

        protected abstract void OnDispose();
    }
}

