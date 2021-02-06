using System.IO;
using System.IO.Abstractions;
using ProductMetadata.Manifest;

namespace ProductMetadata.Services
{
    public abstract class UpdateManifestBuilder<T> : IAvailableManifestBuilder
    {
        public IAvailableProductManifest Build(ProductManifestLocation manifestLocation, IFileInfo manifestFile)
        {
            var manifestModel = SerializeManifestFile(manifestFile);
            if (manifestModel is null)
                throw new ManifestException($"Failed to get manifest from '{manifestFile.FullName}'.");
            var catalog = BuildManifestCatalog(manifestModel, manifestLocation);
            return AvailableProductManifest.FromCatalog(manifestLocation.Product, catalog);
        }

        protected abstract ICatalog BuildManifestCatalog(T manifestModel, ProductManifestLocation manifestLocation);

        protected abstract T SerializeManifestModel(Stream manifestData);

        private T SerializeManifestFile(IFileInfo manifestFile)
        {
            using var fileStream = manifestFile.OpenRead();
            return SerializeManifestModel(fileStream);
        }
    }
}