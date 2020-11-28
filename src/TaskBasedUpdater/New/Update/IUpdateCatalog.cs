using System.Collections.Generic;
using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New.Update
{
    public interface IUpdateCatalog : ICatalog
    {
        IProductReference Product { get; }
        UpdateRequestAction RequestAction { get; }
        IEnumerable<IUpdateItem> ItemsToInstall { get; }
        IEnumerable<IUpdateItem> ItemsToKeep { get; }
        IEnumerable<IUpdateItem> ItemsToDelete { get; }
    }
}