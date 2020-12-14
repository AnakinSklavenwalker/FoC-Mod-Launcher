using SimplePipeline;
using TaskBasedUpdater.Component;

namespace TaskBasedUpdater.Tasks
{
    public interface IUpdaterTask : IPipelineTask
    {
        public ProductComponent ProductComponent { get; }
    }
}
