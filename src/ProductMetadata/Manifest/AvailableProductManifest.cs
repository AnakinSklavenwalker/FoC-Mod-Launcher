using System.Collections.Generic;
using System.Linq;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata.Manifest
{
    public class AvailableProductManifest : Catalog, IAvailableProductManifest
    {
        public IProductReference Product { get; }

        public AvailableProductManifest(IProductReference product, IEnumerable<ProductComponent> items) : base(items)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }

        public static IAvailableProductManifest FromCatalog(IProductReference product, ICatalog catalog)
        {
            Requires.NotNull(catalog, nameof(catalog));
            Requires.NotNull(product, nameof(product));
            return new AvailableProductManifest(product, catalog.Items.ToList());
        }
    }
}