using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SimplePipeline.Tasks
{
    public abstract class PipelineTask : IPipelineTask
    {
        internal bool IsDisposed { get; private set; }

        protected internal ILogger? Logger { get; }

        public Exception? Error { get; internal set; }

        protected PipelineTask(ILogger? logger = null)
        {
            Logger = logger;
        }

        ~PipelineTask()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Run(CancellationToken token)
        {
            Logger?.LogTrace($"BEGIN: {this}");
            try
            {
                RunCore(token);
                Logger?.LogTrace($"END: {this}");
            }
            catch (OperationCanceledException ex)
            {
                Error = ex.InnerException;
                throw;
            }
            catch (StopTaskRunnerException)
            {
                throw;
            }
            catch (PipelineException ex)
            {
                Error = ex;
                throw;
            }
            catch (AggregateException ex)
            {
                if (!ex.IsExceptionType<OperationCanceledException>())
                    LogFaultException(ex);
                else
                    Error = ex.InnerExceptions.FirstOrDefault(p => p.IsExceptionType<OperationCanceledException>())?.InnerException;
                throw;
            }
            catch (Exception e)
            {
                LogFaultException(e);
                throw;
            }
        }
        
        public override string ToString()
        {
            return GetType().Name;
        }

        protected abstract void RunCore(CancellationToken token);

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return; 
            IsDisposed = true;
        }
        
        private void LogFaultException(Exception ex)
        { 
            Error = ex; 
            Logger?.LogError(ex, ex.InnerException?.Message ?? ex.Message);
        }
    }
}