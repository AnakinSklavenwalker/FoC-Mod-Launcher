using ProductMetadata.Component;

namespace ProductMetadata.Manifest
{
    public interface IManifest : ICatalog<IProductComponent>
    {
        IProductReference Product { get; }
    }
}