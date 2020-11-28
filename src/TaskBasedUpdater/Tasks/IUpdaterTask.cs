using SimplePipeline;
using TaskBasedUpdater.ProductComponent;

namespace TaskBasedUpdater.Tasks
{
    public interface IUpdaterTask : IPipelineTask
    {
        public IUpdateItem UpdateItem { get; }
    }
}
