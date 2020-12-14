using System.Collections.Generic;
using TaskBasedUpdater.Component;
using Validation;

namespace TaskBasedUpdater.New
{
    public abstract class Catalog : ICatalog
    {
        public IEnumerable<ProductComponent> Items { get; }

        protected Catalog(IEnumerable<ProductComponent> components)
        {
            Requires.NotNull(components, nameof(components));
            Items = components;
        }
    }
}