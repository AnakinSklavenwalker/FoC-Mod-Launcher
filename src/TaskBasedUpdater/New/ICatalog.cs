using System.Collections.Generic;
using TaskBasedUpdater.ProductComponent;

namespace TaskBasedUpdater.New
{
    public interface ICatalog
    {
        IEnumerable<IUpdateItem> Items { get; }
    }
}