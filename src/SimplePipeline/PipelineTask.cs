using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace SimplePipeline
{
    public abstract class PipelineTask : IPipelineTask
    {
        internal bool IsDisposed { get; private set; }

        protected internal ILogger? Logger { get; private set; }

        protected internal IServiceProvider ServiceProvider { get; private set; }

        public Exception? Error { get; internal set; }

        protected PipelineTask(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            ServiceProvider = serviceProvider;
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
            Initialize();
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

        private void Initialize()
        {
            Logger = ServiceProvider.GetService<ILogger>();
        }

        private void LogFaultException(Exception ex)
        { 
            Error = ex; 
            Logger?.LogError(ex, ex.InnerException?.Message ?? ex.Message);
        }
    }
}