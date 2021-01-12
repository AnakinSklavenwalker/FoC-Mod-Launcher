using System.Collections.Generic;
using TaskBasedUpdater.New.Product.Component;
using Validation;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public class InstalledProductManifest : Catalog, IInstalledProductManifest
    {
        public IProductReference Product { get; }

        public InstalledProductManifest(IProductReference product, IEnumerable<ProductComponent> items) : base(items)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }
    }
}