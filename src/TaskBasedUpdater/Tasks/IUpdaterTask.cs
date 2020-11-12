using SimplePipeline;
using TaskBasedUpdater.Component;

namespace TaskBasedUpdater.Tasks
{
    public interface IUpdaterTask : IPipelineTask
    {
        public IUpdateItem UpdateItem { get; }
    }
}
