using System.Collections.Generic;
using System.Linq;

namespace ProductMetadata.Component
{
    public abstract class InstallableComponent : ProductComponent, IInstallableComponent
    {
        public long DownloadSize => !OriginInfos.Any() ? 0 : OriginInfos.Sum(x => x.Size ?? 0);
        public IList<OriginInfo> OriginInfos { get; init; } = new List<OriginInfo>();
        public IList<Condition> DetectConditions { get; init; } = new List<Condition>();
        public InstallSizes InstallSizes { get; init; }
        
        protected InstallableComponent(IProductComponentIdentity identity) 
            : base(identity)
        {
        }
    }
}