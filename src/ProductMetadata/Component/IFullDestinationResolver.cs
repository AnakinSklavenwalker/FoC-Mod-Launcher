namespace ProductMetadata.Component
{
    public interface IFullDestinationResolver
    {
        string GetFullDestination(string destination, string? basePath);

        string GetFullDestination(ProductComponent component, bool includeFileName, string? basePath);
    }
}