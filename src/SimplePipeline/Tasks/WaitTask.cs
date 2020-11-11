using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimplePipeline.Runners;
using Validation;

namespace SimplePipeline.Tasks
{
    public class WaitTask : PipelineTask
    {
        private readonly AsyncTaskRunner _runner;

        public WaitTask(IServiceProvider serviceProvider, AsyncTaskRunner runner) : base(serviceProvider)
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
