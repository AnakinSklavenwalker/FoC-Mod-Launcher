using System.Collections.Generic;
using TaskBasedUpdater.Component;
using Validation;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public class AvailableProductManifest : IAvailableProductManifest
    {
        public IEnumerable<ProductComponent> Items { get; }
        public IProductReference Product { get; }

        public AvailableProductManifest(IProductReference product, IEnumerable<ProductComponent> items)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(items, nameof(items));
            Product = product;
            Items = items;
        }
    }
}