using TaskBasedUpdater.New.Update;

namespace TaskBasedUpdater.New.Product
{
    public interface IProductCatalogService
    {
        IInstalledProductCatalog GetInstalledProductCatalog(IInstalledProduct product);

        IAvailableProductCatalog? GetAvailableProductCatalog(IUpdateRequest product);
    }
}