using System;
using TaskBasedUpdater.New.Product.Manifest;
using TaskBasedUpdater.New.Update;

namespace TaskBasedUpdater.New.Product
{
    public interface IProductService
    {
        IInstalledProduct GetCurrentInstance();

        void UpdateCurrentInstance(IInstalledProduct product);

        IProductReference CreateProductReference(Version? newVersion, ProductReleaseType newReleaseType);

        IInstalledProductCatalog GetInstalledProductCatalog();

        IAvailableProductManifest GetAvailableProductCatalog(UpdateRequest updateRequest);
    }
}