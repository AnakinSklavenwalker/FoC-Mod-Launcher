using System.IO;
using System.IO.Abstractions;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public abstract class UpdateManifestBuilder<T> : IAvailableManifestBuilder
    {
        public IAvailableProductManifest Build(IProductReference product, IFileInfo manifestFile)
        {
            var manifestModel = SerializeManifestFile(manifestFile);
            if (manifestModel is null)
                throw new ManifestException($"Failed to get manifest from '{manifestFile.FullName}'.");
            var catalog = BuildManifestCatalog(manifestModel);
            return AvailableProductManifest.FromCatalog(product, catalog);
        }

        protected abstract ICatalog BuildManifestCatalog(T manifestModel);

        protected abstract T SerializeManifestModel(Stream manifestData);

        private T SerializeManifestFile(IFileInfo manifestFile)
        {
            using var fileStream = manifestFile.OpenRead();
            return SerializeManifestModel(fileStream);
        }
    }
}