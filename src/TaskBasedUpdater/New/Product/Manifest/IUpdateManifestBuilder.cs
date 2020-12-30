using System.IO.Abstractions;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public interface IAvailableManifestBuilder
    {
        IAvailableProductManifest Build(IProductReference product, IFileInfo manifestFile);
    }
}