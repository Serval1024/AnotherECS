using System;

namespace AnotherECS.Core
{
    public unsafe abstract class BDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        ~BDisposable()
            => Dispose(false);

        public void Dispose()
            => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                OnDispose();
                GC.SuppressFinalize(this);
            }
        }

        protected abstract void OnDispose();
    }
}

