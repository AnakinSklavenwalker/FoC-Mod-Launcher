using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New.Update
{
    public interface IUpdateCatalogBuilder
    {
        IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductCatalog availableCatalog);
    }
}