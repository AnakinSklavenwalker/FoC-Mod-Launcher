using System;
using System.Threading;

namespace SimplePipeline
{
    internal interface IPipelineTask : IDisposable
    {
        Exception? Error { get; }

        void Run(CancellationToken token);
    }
}