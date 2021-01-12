using SimplePipeline;
using TaskBasedUpdater.New.Product.Component;

namespace TaskBasedUpdater.Tasks
{
    public interface IUpdaterTask : IPipelineTask
    {
        public ProductComponent ProductComponent { get; }
    }
}
