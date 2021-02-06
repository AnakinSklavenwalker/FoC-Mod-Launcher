using ProductMetadata.Component;

namespace ProductMetadata.Services
{
    public interface IFullDestinationResolver
    {
        string GetFullDestination(string destination, string? basePath);

        string GetFullDestination(ProductComponent component, bool includeFileName, string? basePath);
    }
}