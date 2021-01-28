using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SimplePipeline
{
    public class PipelineRunner : IPipelineRunner
    {
        public event EventHandler<TaskEventArgs>? Error;

        protected readonly List<IPipelineTask> TaskList;

        protected ConcurrentQueue<IPipelineTask> TaskQueue { get; }

        protected ILogger? Logger { get; }

        public IReadOnlyList<IPipelineTask> Tasks => new ReadOnlyCollection<IPipelineTask>(TaskList);

        internal bool IsCancelled { get; private set; }

        public PipelineRunner(ILogger? logger = null)
        {
            TaskQueue = new ConcurrentQueue<IPipelineTask>();
            TaskList = new List<IPipelineTask>();
            Logger = logger;
        }

        public void Run(CancellationToken token)
        {
            Invoke(token);
        }

        public void Queue(IPipelineTask activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            TaskQueue.Enqueue(activity);
        }

        public IEnumerator<IPipelineTask> GetEnumerator()
        {
            return TaskQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TaskQueue.GetEnumerator();
        }

        protected virtual void Invoke(CancellationToken token)
        {
            var alreadyCancelled = false;
            TaskList.AddRange(TaskQueue);
            while (TaskQueue.TryDequeue(out var task))
            {
                try
                {
                    ThrowIfCancelled(token);
                    task.Run(token);
                }
                catch (StopTaskRunnerException)
                {
                    Logger?.LogTrace("Stop subsequent tasks");
                    break;
                }
                catch (Exception e)
                {
                    if (!alreadyCancelled)
                    {
                        if (e.IsExceptionType<OperationCanceledException>())
                            Logger?.LogTrace($"Task {task} cancelled");
                        else
                            Logger?.LogTrace(e, $"Task {task} threw an exception: {e.GetType()}: {e.Message}");
                    }

                    var error = new TaskEventArgs(task)
                    {
                        Cancel = token.IsCancellationRequested || IsCancelled ||
                                 e.IsExceptionType<OperationCanceledException>()
                    };
                    if (error.Cancel)
                        alreadyCancelled = true;
                    OnError(error);
                }
            }
        }

        protected virtual void OnError(TaskEventArgs e)
        {
            Error?.Invoke(this, e);
            if (!e.Cancel)
                return;
            IsCancelled |= e.Cancel;
        }

        protected void ThrowIfCancelled(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (IsCancelled)
                throw new OperationCanceledException(token);
        }
    }
}