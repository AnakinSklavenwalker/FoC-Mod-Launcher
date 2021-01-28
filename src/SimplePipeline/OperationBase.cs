using System.Threading;

namespace SimplePipeline
{
    public abstract class OperationBase : IOperation
    {
        private bool? _planSuccessful;

        public bool Plan()
        {
            if (_planSuccessful.HasValue)
                return _planSuccessful.Value;
            Initialize();
            _planSuccessful = PlanOperation();
            return _planSuccessful.Value;
        }


        public void Run(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Initialize();
            if (!Plan())
                return;
            RunCore(token);
        }

        protected abstract bool PlanOperation();

        protected abstract void RunCore(CancellationToken token);

        protected virtual void Initialize()
        {
        }
    }
}