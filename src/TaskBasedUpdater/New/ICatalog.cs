using System.Collections.Generic;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.New
{
    public interface ICatalog
    {
        IEnumerable<IUpdateItem> Items { get; }
    }
}