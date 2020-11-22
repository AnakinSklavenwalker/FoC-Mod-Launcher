using System.Collections.Generic;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.New
{
    public class UpdateCatalog : IUpdateCatalog
    {
        public IEnumerable<IUpdateItem> Items { get; }
        public IProductReference Product { get; }
        public IEnumerable<IUpdateItem> ItemsToInstall { get; }
        public IEnumerable<IUpdateItem> ItemsToKeep { get; }
        public IEnumerable<IUpdateItem> ItemsToDelete { get; }
    }
}