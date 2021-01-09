using SimplePipeline;

namespace TaskBasedUpdater.Tasks
{
    public interface IUpdaterTask : IPipelineTask
    {
        public ProductComponent ProductComponent { get; }
    }
}
