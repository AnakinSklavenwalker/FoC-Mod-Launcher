using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimplePipeline.Runners
{
    internal class TaskRunner : IEnumerable<IPipelineTask>
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly List<IPipelineTask> _tasks;

        public event EventHandler<TaskEventArgs>? Error;

        protected ConcurrentQueue<IPipelineTask> TaskQueue { get; }

        protected ILogger? Logger { get; private set; }

        internal IList<IPipelineTask> Tasks => _tasks;

        internal bool IsCancelled { get; private set; }

        public TaskRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            TaskQueue = new ConcurrentQueue<IPipelineTask>();
            _tasks = new List<IPipelineTask>();
        }

        public void Run(CancellationToken token)
        {
            Initializer();
            Invoke(token);
        }

        private void Initializer()
        {
            Logger = _serviceProvider.GetService<ILogger>();
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
            _tasks.AddRange(TaskQueue);
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