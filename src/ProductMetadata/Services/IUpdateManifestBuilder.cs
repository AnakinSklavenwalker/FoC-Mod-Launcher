using System.IO.Abstractions;
using ProductMetadata.Manifest;

namespace ProductMetadata.Services
{
    public interface IAvailableManifestBuilder
    {
        IAvailableProductManifest Build(ProductManifestLocation manifestLocation, IFileInfo manifestFile);
    }
}