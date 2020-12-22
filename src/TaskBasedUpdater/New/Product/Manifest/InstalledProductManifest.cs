using System.Collections.Generic;
using TaskBasedUpdater.Component;
using Validation;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public class InstalledProductManifest : IInstalledProductManifest
    {
        public IEnumerable<ProductComponent> Items { get; }
        public IProductReference Product { get; }

        public InstalledProductManifest(IProductReference product, IEnumerable<ProductComponent> items)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(items, nameof(items));
            Product = product;
            Items = items;
        }
    }
}