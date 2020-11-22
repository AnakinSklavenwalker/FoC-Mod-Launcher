namespace TaskBasedUpdater.New.Product
{
    public interface IInstalledProductCatalog : ICatalog
    {
        IInstalledProduct Product { get; }
    }
}