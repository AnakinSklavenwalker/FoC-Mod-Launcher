namespace TaskBasedUpdater.New.Product
{
    public interface IAvailableProductCatalog : ICatalog
    {
        IProductReference Product { get; }
    }
}