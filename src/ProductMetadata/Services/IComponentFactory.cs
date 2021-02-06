using ProductMetadata.Component;

namespace ProductMetadata.Services
{
    public interface IComponentFactory
    {
        ProductComponent Create(ProductComponent manifestComponent, IInstalledProduct product);
    }
}