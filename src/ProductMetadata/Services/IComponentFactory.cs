using ProductMetadata.Component;

namespace ProductMetadata.Services
{
    public interface IComponentDetectorFactory
    {
        IComponentDetector GetDetector(ComponentType type);
    }
}