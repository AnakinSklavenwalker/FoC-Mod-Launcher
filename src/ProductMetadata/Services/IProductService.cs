using System;
using ProductMetadata.Manifest;

namespace ProductMetadata.Services
{
    public interface IProductService
    {
        IInstalledProduct GetCurrentInstance();

        void UpdateCurrentInstance(IInstalledProduct product);

        IProductReference CreateProductReference(Version? newVersion, string? branch);

        IInstalledProductCatalog GetInstalledProductCatalog();

        IManifest GetAvailableProductManifest(ManifestLocation updateRequest);
    }
}