using System;
using System.Threading;

namespace SimplePipeline
{
    public interface IPipelineTask : IDisposable
    {
        Exception? Error { get; }

        void Run(CancellationToken token);
    }
}