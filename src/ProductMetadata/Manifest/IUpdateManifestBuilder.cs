using System.IO.Abstractions;

namespace ProductMetadata.Manifest
{
    public interface IAvailableManifestBuilder
    {
        IAvailableProductManifest Build(ProductManifestLocation manifestLocation, IFileInfo manifestFile);
    }
}