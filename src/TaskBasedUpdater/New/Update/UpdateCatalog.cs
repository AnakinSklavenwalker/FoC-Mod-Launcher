using System.Collections.Generic;
using System.Linq;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.UpdateItem;
using Validation;

namespace TaskBasedUpdater.New.Update
{
    public class UpdateCatalog : IUpdateCatalog
    {
        public IEnumerable<IUpdateItem> Items { get; }
        public IProductReference Product { get; }
        public UpdateRequestAction RequestAction { get; }
        public IEnumerable<IUpdateItem> ItemsToInstall => Items.Where(x => x.RequiredAction == UpdateAction.Update);
        public IEnumerable<IUpdateItem> ItemsToKeep => Items.Where(x => x.RequiredAction == UpdateAction.Keep);
        public IEnumerable<IUpdateItem> ItemsToDelete => Items.Where(x => x.RequiredAction == UpdateAction.Delete);

        public UpdateCatalog(IProductReference product, IEnumerable<IUpdateItem> items, UpdateRequestAction requestAction)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(items, nameof(items));
            Product = product;
            RequestAction = requestAction;
            Items = items;
        }
    }
}