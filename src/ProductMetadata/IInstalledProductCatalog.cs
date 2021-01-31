namespace ProductMetadata
{
    public interface IInstalledProductCatalog : ICatalog
    {
        IInstalledProduct Product { get; }
    }
}