using System;

namespace TaskBasedUpdater
{
    public abstract class DisposableObject : IDisposable
    {
        public bool IsDisposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected void VerifyNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}