using ProductMetadata.Component;
using SimplePipeline;

namespace ProductUpdater.Tasks
{
    public interface IComponentTask : IPipelineTask
    {
        public ProductComponent ProductComponent { get; }
    }
}
