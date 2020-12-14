using System.Collections.Generic;
using System.Linq;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New.Product;
using Validation;

namespace TaskBasedUpdater.New.Update
{
    public class UpdateCatalog : IUpdateCatalog
    {
        public IEnumerable<ProductComponent> Items { get; }
        public IProductReference Product { get; }
        public UpdateRequestAction RequestAction { get; }
        public IEnumerable<ProductComponent> ComponentsToInstall => Items.Where(x => x.RequiredAction == ComponentAction.Update);
        public IEnumerable<ProductComponent> ComponentsToKeep => Items.Where(x => x.RequiredAction == ComponentAction.Keep);
        public IEnumerable<ProductComponent> ComponentsToDelete => Items.Where(x => x.RequiredAction == ComponentAction.Delete);

        public UpdateCatalog(IProductReference product, IEnumerable<ProductComponent> components, UpdateRequestAction requestAction)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(components, nameof(components));
            Product = product;
            RequestAction = requestAction;
            Items = components;
        }
    }
}