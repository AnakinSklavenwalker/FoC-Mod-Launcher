using System.Threading;
using Microsoft.Extensions.Logging;

namespace SimplePipeline.Tasks
{
    public class AcquireMutexTask : PipelineTask
    {
        private Mutex? _mutex;

        internal string MutexName { get; }

        public AcquireMutexTask(string? name = null, ILogger? logger = null) : base(logger)
        {
            MutexName = name ?? Utilities.GlobalPipelineMutex;
        }

        public override string ToString()
        {
            return $"Acquiring mutex: {MutexName}";
        }

        protected override void RunCore(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            _mutex = Utilities.CheckAndSetGlobalMutex(MutexName);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.Dispose(disposing);
        }
    }
}