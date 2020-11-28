using System.Collections.Generic;

namespace TaskBasedUpdater.New
{
    public interface ICatalog
    {
        IEnumerable<IUpdateItem> Items { get; }
    }
}