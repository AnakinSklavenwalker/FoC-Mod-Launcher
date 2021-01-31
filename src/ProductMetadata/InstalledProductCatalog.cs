﻿using System.Collections.Generic;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata
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