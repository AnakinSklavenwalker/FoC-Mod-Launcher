using System.Collections.Generic;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata.Manifest
{
    public class Manifest : IManifest
    {
        IEnumerable<IProductComponentIdentity> ICatalog.Items => Items;

        public IEnumerable<IProductComponent> Items { get; }
       
        public IProductReference Product { get; }


        public Manifest(IProductReference product, IEnumerable<IProductComponent> items)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(items, nameof(items));
            Product = product;
            Items = items;
        }
    }
}