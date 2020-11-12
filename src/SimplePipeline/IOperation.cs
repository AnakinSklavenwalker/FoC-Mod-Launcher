using System.Threading;

namespace SimplePipeline
{
    public interface IOperation
    {
        bool Plan();

        void Run(CancellationToken token = default);
    }
}