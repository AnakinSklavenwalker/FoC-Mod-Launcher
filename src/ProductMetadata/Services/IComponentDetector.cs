using ProductMetadata.Component;

namespace ProductMetadata.Services
{
    public interface IComponentDetector
    {
        IProductComponent Find(IProductComponent manifestComponent, IInstalledProduct product);
    }
}