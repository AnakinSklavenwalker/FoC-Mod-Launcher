using System.Collections.Generic;
using Validation;

namespace ProductMetadata.Component
{
    /// <summary>
    /// A component representing a single file.
    /// <remarks>
    /// The property <see cref="IInstallableComponent.DetectConditions"/> will not get shall not get used for installation detection.
    /// </remarks>
    /// </summary>
    public class SingleFileComponent : FileComponent
    {
        public override ComponentType Type => ComponentType.File;

        /// <summary>
        /// Relative Path without file name where the file shall get installed to. May contain environment variables.
        /// </summary>
        public string Path { get; }

        public SingleFileComponent(IProductComponentIdentity identity, string path, IList<FileItem> files) 
            : base(identity, files)
        {
            Requires.NotNullOrEmpty(path, nameof(path));
            Path = path;
        }
    }
}