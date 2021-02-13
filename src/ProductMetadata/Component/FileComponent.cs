using System.Collections.Generic;

namespace ProductMetadata.Component
{
    public abstract class FileComponent : InstallableComponent
    {
        public IList<FileItem> Files { get; } = new List<FileItem>();

        protected FileComponent(IProductComponentIdentity identity) : base(identity)
        {
        }
    }
}