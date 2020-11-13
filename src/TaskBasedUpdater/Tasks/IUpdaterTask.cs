using SimplePipeline;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.Tasks
{
    public interface IUpdaterTask : IPipelineTask
    {
        public IUpdateItem UpdateItem { get; }
    }
}
