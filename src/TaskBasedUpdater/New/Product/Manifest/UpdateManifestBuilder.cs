using System.IO;
using System.IO.Abstractions;
using TaskBasedUpdater.New.Update;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public abstract class UpdateManifestBuilder<T> : IAvailableManifestBuilder
    {
        public IAvailableProductManifest Build(UpdateRequest updateRequest, IFileInfo manifestFile)
        {
            var manifestModel = SerializeManifestFile(manifestFile);
            if (manifestModel is null)
                throw new ManifestException($"Failed to get manifest from '{manifestFile.FullName}'.");
            var catalog = BuildManifestCatalog(manifestModel, updateRequest);
            return AvailableProductManifest.FromCatalog(updateRequest.Product, catalog);
        }

        protected abstract ICatalog BuildManifestCatalog(T manifestModel, UpdateRequest updateRequest);

        protected abstract T SerializeManifestModel(Stream manifestData);

        private T SerializeManifestFile(IFileInfo manifestFile)
        {
            using var fileStream = manifestFile.OpenRead();
            return SerializeManifestModel(fileStream);
        }
    }
}