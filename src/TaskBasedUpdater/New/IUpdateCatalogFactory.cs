namespace TaskBasedUpdater.New
{
    public interface IUpdateCatalogFactory
    {
        string CatalogPath { get; }

        IUpdateCatalog Load();
    }
}