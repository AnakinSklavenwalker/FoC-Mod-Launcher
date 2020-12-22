using System;
using TaskBasedUpdater.New.Update;

namespace TaskBasedUpdater.New.Product
{
    public interface IProductService
    {
        IInstalledProduct GetCurrentInstance();

        void UpdateCurrentInstance(IInstalledProduct product);

        IProductReference CreateProductReference(Version? newVersion, ProductReleaseType newReleaseType);

        IInstalledProductCatalog GetInstalledProductCatalog();

        IAvailableProductCatalog GetAvailableProductCatalog(UpdateRequest updateRequest);
    }
}