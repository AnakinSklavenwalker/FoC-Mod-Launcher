using ProductMetadata.Component;

namespace ProductMetadata.Services.Detectors
{
    public interface IComponentDetector
    {
        IProductComponent Find(IProductComponent manifestComponent, IInstalledProduct product);
    }
}