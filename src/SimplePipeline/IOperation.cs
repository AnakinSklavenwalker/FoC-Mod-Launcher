using System.Threading;

namespace SimplePipeline
{
    internal interface IOperation
    {
        bool Plan();

        void Run(CancellationToken token = default);
    }
}