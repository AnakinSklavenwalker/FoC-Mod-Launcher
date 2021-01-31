using ProductMetadata;
using ProductMetadata.Manifest;

namespace ProductUpdater.New.Update
{
    public interface IUpdateCatalogBuilder
    {
        IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductManifest availableCatalog);
    }
}