namespace ProductMetadata.Manifest
{
    public interface IManifest : ICatalog
    {
        IProductReference Product { get; }
    }
}