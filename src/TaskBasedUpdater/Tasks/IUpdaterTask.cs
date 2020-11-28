using SimplePipeline;

namespace TaskBasedUpdater.Tasks
{
    public interface IUpdaterTask : IPipelineTask
    {
        public IUpdateItem UpdateItem { get; }
    }
}
