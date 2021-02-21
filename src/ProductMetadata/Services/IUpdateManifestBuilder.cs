using System.IO.Abstractions;
using ProductMetadata.Manifest;

namespace ProductMetadata.Services
{
    public interface IAvailableManifestBuilder
    {
        IManifest Build(IFileInfo manifestFile, IProductReference product);
    }
}