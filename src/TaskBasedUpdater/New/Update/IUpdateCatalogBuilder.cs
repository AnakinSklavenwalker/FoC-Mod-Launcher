using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Product.Manifest;

namespace TaskBasedUpdater.New.Update
{
    public interface IUpdateCatalogBuilder
    {
        IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductManifest availableCatalog);
    }
}