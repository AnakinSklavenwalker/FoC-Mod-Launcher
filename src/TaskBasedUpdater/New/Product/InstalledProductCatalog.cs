using System.Collections.Generic;
using TaskBasedUpdater.New.Product.Component;
using Validation;

namespace TaskBasedUpdater.New.Product
{
    public class InstalledProductCatalog : Catalog, IInstalledProductCatalog
    {
        public IInstalledProduct Product { get; }

        public InstalledProductCatalog(IInstalledProduct product, IEnumerable<ProductComponent> components) : base(components)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }
    }
}