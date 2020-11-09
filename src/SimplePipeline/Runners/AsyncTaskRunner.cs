using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SimplePipeline.Runners
{
    internal class AsyncTaskRunner : TaskRunner
    {
        private readonly ConcurrentBag<Exception> _exceptions;
        private readonly Task[] _tasks;
        private CancellationToken _cancel;

        internal int WorkerCount { get; }

        internal AggregateException? Exception => _exceptions.Count > 0 ? new AggregateException(_exceptions) : null;

        public AsyncTaskRunner(IServiceProvider serviceProvider, int workerCount) : base(serviceProvider)
        {
            if (workerCount < 1)
                throw new ArgumentOutOfRangeException(nameof(workerCount));
            WorkerCount = workerCount;
            _exceptions = new ConcurrentBag<Exception>();
            _tasks = new Task[workerCount];
        }

        public void Wait()
        {
            Wait(Timeout.InfiniteTimeSpan);
            var exception = Exception;
            if (exception != null)
                throw exception;
        }

        internal void Wait(TimeSpan timeout)
        {
            Task.WaitAll(_tasks, timeout);
        }

        protected override void Invoke(CancellationToken token)
        {
            ThrowIfCancelled(token);
            Tasks.AddRange(TaskQueue);
            _cancel = token;
            for (var index = 0; index < WorkerCount; ++index) 
                _tasks[index] = Task.Run(InvokeThreaded, default);
        }

        private void InvokeThreaded()
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancel);
            var canceled = false;
            while (TaskQueue.TryDequeue(out var task))
            {
                try
                {
                    ThrowIfCancelled(_cancel);
                    task.Run(_cancel);
                }
                catch (Exception ex)
                {
                    _exceptions.Add(ex);
                    if (!canceled)
                    {
                        if (ex.IsExceptionType<OperationCanceledException>())
                            Logger?.LogTrace($"Activity threw exception {ex.GetType()}: {ex.Message}" + Environment.NewLine + $"{ex.StackTrace}");
                        else
                            Logger?.LogTrace(ex, $"Activity threw exception {ex.GetType()}: {ex.Message}");
                    }
                    var e = new TaskEventArgs(task)
                    {
                        Cancel = _cancel.IsCancellationRequested || IsCancelled || ex.IsExceptionType<OperationCanceledException>()
                    };
                    OnError(e);
                    if (e.Cancel)
                    {
                        canceled = true;
                        linkedTokenSource.Cancel();
                    }
                }
            }
        }
    }
}