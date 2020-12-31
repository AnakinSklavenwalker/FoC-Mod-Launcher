using System.IO.Abstractions;
using TaskBasedUpdater.New.Update;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public interface IAvailableManifestBuilder
    {
        IAvailableProductManifest Build(UpdateRequest updateRequest, IFileInfo manifestFile);
    }
}