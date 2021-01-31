using System;
using ProductMetadata.Manifest;

namespace ProductMetadata.Services
{
    public interface IProductService
    {
        IInstalledProduct GetCurrentInstance();

        void UpdateCurrentInstance(IInstalledProduct product);

        IProductReference CreateProductReference(Version? newVersion, ProductReleaseType newReleaseType);

        IInstalledProductCatalog GetInstalledProductCatalog();

        IAvailableProductManifest GetAvailableProductManifest(ProductManifestLocation updateRequest);
    }
}