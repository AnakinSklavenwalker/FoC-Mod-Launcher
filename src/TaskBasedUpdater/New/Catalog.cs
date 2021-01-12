using System.Collections.Generic;
using TaskBasedUpdater.New.Product.Component;
using Validation;

namespace TaskBasedUpdater.New
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