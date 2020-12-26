using System.Collections.Generic;
using TaskBasedUpdater.Component;
using Validation;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public class AvailableProductManifest : Catalog, IAvailableProductManifest
    {
        public IProductReference Product { get; }

        public AvailableProductManifest(IProductReference product, IEnumerable<ProductComponent> items) : base(items)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }
    }
}