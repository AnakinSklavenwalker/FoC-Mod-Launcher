using SimplePipeline;
using TaskBasedUpdater.New.Product.Component;

namespace TaskBasedUpdater.Tasks
{
    public interface IComponentTask : IPipelineTask
    {
        public ProductComponent ProductComponent { get; }
    }
}
