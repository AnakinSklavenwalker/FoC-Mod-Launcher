using System.Collections.Generic;
using Validation;

namespace TaskBasedUpdater.New
{
    public abstract class Catalog : ICatalog
    {
        public IEnumerable<IUpdateItem> Items { get; }

        protected Catalog(IEnumerable<IUpdateItem> updateItems)
        {
            Requires.NotNull(updateItems, nameof(updateItems));
            Items = updateItems;
        }
    }
}