using System;
using System.Collections.Generic;
using System.Threading;

namespace SimplePipeline
{
    public interface IPipelineRunner : IEnumerable<IPipelineTask>
    {
        event EventHandler<TaskEventArgs>? Error;

        void Run(CancellationToken token);

        void Queue(IPipelineTask activity);
    }
}