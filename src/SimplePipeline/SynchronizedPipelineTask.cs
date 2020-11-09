using System;
using System.Threading;

namespace SimplePipeline
{
    internal abstract class SynchronizedPipelineTask : PipelineTask
    {
        public event EventHandler<EventArgs>? Canceled;

        private readonly ManualResetEvent _handle;

        protected SynchronizedPipelineTask()
        {
            _handle = new ManualResetEvent(false);
        }

        ~SynchronizedPipelineTask()
        {
            Dispose(false);
        }

        public void Wait()
        {
            Wait(Timeout.InfiniteTimeSpan);
        }

        internal void Wait(TimeSpan timeout)
        {
            if (!_handle.WaitOne(timeout))
                throw new TimeoutException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _handle.Dispose();
            base.Dispose(disposing);
        }

        protected sealed override void ExecuteTask(CancellationToken token)
        {
            try
            {
                SynchronizedInvoke(token);
            }
            catch (Exception ex)
            {
                if (ex.IsExceptionType<OperationCanceledException>()) 
                    Canceled?.Invoke(this, new EventArgs());
                throw;
            }
            finally
            {
                _handle.Set();
            }
        }

        protected abstract void SynchronizedInvoke(CancellationToken token);
    }
}