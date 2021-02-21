using System.IO;
using System.IO.Abstractions;
using ProductMetadata.Manifest;
using Validation;

namespace ProductMetadata.Services
{
    public abstract class ManifestBuilder<T> : IAvailableManifestBuilder
    {
        public IManifest Build(IFileInfo manifestFile, IProductReference product)
        {
            Requires.NotNull(manifestFile, nameof(manifestFile));
            Requires.NotNull(product, nameof(product));
            var manifestModel = SerializeManifestFile(manifestFile, product);
            if (manifestModel is null)
                throw new ManifestException($"Failed to get manifest from '{manifestFile.FullName}'.");
            return BuildManifestCatalog(manifestModel, product);
        }

        protected abstract IManifest BuildManifestCatalog(T manifestModel, IProductReference product);

        protected abstract T SerializeManifestModel(Stream manifestData, IProductReference productReference);

        private T SerializeManifestFile(IFileInfo manifestFile, IProductReference product)
        {
            using var fileStream = manifestFile.OpenRead();
            return SerializeManifestModel(fileStream, product);
        }
    }
}