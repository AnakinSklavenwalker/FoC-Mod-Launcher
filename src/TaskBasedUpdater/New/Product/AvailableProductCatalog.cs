using System.Collections.Generic;
using TaskBasedUpdater.Component;
using Validation;

namespace TaskBasedUpdater.New.Product
{
    public class AvailableProductCatalog : Catalog, IAvailableProductCatalog
    {
        public IProductReference Product { get; }

        public AvailableProductCatalog(IProductReference product, IEnumerable<ProductComponent> components) : base(components)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }
    }
}