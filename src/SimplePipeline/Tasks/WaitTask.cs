using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Validation;

namespace SimplePipeline.Tasks
{
    public class WaitTask : PipelineTask
    {
        private readonly AsyncPipelineRunner _runner;

        public WaitTask(AsyncPipelineRunner runner, ILogger? logger = null) : base(logger)
        {
            Requires.NotNull(runner, nameof(runner));
            _runner = runner;
        }

        public override string ToString() => "Waiting for other tasks";

        protected override void RunCore(CancellationToken token)
        {
            try
            {
                _runner.Wait();
            }
            catch
            {
                Logger?.LogTrace("Wait task is stopping all subsequent tasks");
                throw new StopTaskRunnerException();
            }
        }
    }
}
