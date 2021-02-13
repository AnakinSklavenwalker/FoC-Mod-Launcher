using ProductMetadata.Component;

namespace ProductMetadata
{
    public interface IInstalledProductCatalog : ICatalog<IProductComponent>
    {
        IInstalledProduct Product { get; }
    }
}