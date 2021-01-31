using System.Collections.Generic;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata
{
    public class Catalog : ICatalog
    {
        public IEnumerable<ProductComponent> Items { get; }

        public Catalog(IEnumerable<ProductComponent> components)
        {
            Requires.NotNull(components, nameof(components));
            Items = components;
        }
    }
}