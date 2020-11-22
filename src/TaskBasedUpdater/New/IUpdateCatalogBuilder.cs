using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New
{
    public interface IUpdateCatalogBuilder
    {
        IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductCatalog availableCatalog,
            UpdateRequestAction action);
    }
}