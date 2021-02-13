using Validation;

namespace ProductMetadata.Component
{
    public class SingleFileComponent : FileComponent
    {
        public override ComponentType Type => ComponentType.File;

        public string Path { get; }

        public SingleFileComponent(IProductComponentIdentity identity, string path) : base(identity)
        {
            Requires.NotNullOrEmpty(path, nameof(path));
            Path = path;
        }
    }
}