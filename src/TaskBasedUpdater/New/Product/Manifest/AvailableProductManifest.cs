using System.Collections.Generic;
using System.Linq;
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

        public static IAvailableProductCatalog FromCatalog(ICatalog catalog, IProductReference product)
        {
            Requires.NotNull(catalog, nameof(catalog));
            Requires.NotNull(product, nameof(product));
            return new AvailableProductCatalog(product, catalog.Items.ToList());
        }
    }
}