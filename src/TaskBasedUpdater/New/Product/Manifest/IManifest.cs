namespace TaskBasedUpdater.New.Product.Manifest
{
    public interface IManifest : ICatalog
    {
        IProductReference Product { get; }
    }
}