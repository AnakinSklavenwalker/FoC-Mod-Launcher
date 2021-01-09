using System.Collections.Generic;

namespace TaskBasedUpdater.New
{
    public interface ICatalog
    {
        IEnumerable<ProductComponent> Items { get; }
    }
}