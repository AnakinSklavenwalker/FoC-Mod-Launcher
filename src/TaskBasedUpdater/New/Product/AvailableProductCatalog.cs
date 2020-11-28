using System.Collections.Generic;
using Validation;

namespace TaskBasedUpdater.New.Product
{
    public class AvailableProductCatalog : Catalog, IAvailableProductCatalog
    {
        public IProductReference Product { get; }

        public AvailableProductCatalog(IProductReference product, IEnumerable<IUpdateItem> updateItems) : base(updateItems)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }
    }
}