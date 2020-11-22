using System.Collections.Generic;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.New
{
    public interface IUpdateCatalog : ICatalog
    {
        IProductReference Product { get; }

        IEnumerable<IUpdateItem> ItemsToInstall { get; }
        IEnumerable<IUpdateItem> ItemsToKeep { get; }
        IEnumerable<IUpdateItem> ItemsToDelete { get; }
    }
}