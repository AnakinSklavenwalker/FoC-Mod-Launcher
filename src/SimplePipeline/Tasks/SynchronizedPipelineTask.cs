using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SimplePipeline.Tasks
{
    public abstract class SynchronizedPipelineTask : PipelineTask
    {
        public event EventHandler<EventArgs>? Canceled;

        private readonly ManualResetEvent _handle;

        protected SynchronizedPipelineTask(ILogger? logger = null) : base(logger)
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

        public void Wait(TimeSpan timeout)
        {
            if (!_handle.WaitOne(timeout))
                throw new TimeoutException();
        }

        protected abstract void SynchronizedInvoke(CancellationToken token);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _handle.Dispose();
            base.Dispose(disposing);
        }

        protected sealed override void RunCore(CancellationToken token)
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
    }
}