using System.Collections.Generic;
using TaskBasedUpdater.Component;

namespace TaskBasedUpdater.New
{
    public interface ICatalog
    {
        IEnumerable<ProductComponent> Items { get; }
    }
}