using System.Collections.Generic;
using System.Collections.ObjectModel;
using Validation;

namespace ProductMetadata.Component
{
    public abstract class FileComponent : InstallableComponent
    {
        public IReadOnlyList<FileItem> Files { get; }

        protected FileComponent(IProductComponentIdentity identity, IList<FileItem> files) : base(identity)
        {
            Requires.NotNull(files, nameof(files));
            Files = new ReadOnlyCollection<FileItem>(files);
        }
    }
}