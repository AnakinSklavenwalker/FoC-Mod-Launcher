using System.IO;
using System.IO.Abstractions;
using ProductMetadata.Manifest;

namespace ProductMetadata.Services
{
    public abstract class UpdateManifestBuilder<T> : IAvailableManifestBuilder
    {
        public IManifest Build(ManifestLocation manifestLocation, IFileInfo manifestFile)
        {
            var manifestModel = SerializeManifestFile(manifestFile);
            if (manifestModel is null)
                throw new ManifestException($"Failed to get manifest from '{manifestFile.FullName}'.");
            return BuildManifestCatalog(manifestModel, manifestLocation);
        }

        protected abstract IManifest BuildManifestCatalog(T manifestModel, ManifestLocation manifestLocation);

        protected abstract T SerializeManifestModel(Stream manifestData);

        private T SerializeManifestFile(IFileInfo manifestFile)
        {
            using var fileStream = manifestFile.OpenRead();
            return SerializeManifestModel(fileStream);
        }
    }
}