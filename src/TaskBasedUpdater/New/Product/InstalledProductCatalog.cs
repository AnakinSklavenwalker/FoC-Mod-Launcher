using System.Collections.Generic;
using Validation;

namespace TaskBasedUpdater.New.Product
{
    public class InstalledProductCatalog : Catalog, IInstalledProductCatalog
    {
        public IInstalledProduct Product { get; }

        public InstalledProductCatalog(IInstalledProduct product, IEnumerable<IUpdateItem> updateItems) : base(updateItems)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }
    }
}