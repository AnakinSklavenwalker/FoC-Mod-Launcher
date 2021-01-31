using System.Collections.Generic;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata.Manifest
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