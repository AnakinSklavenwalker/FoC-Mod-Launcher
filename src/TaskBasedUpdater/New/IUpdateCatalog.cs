using System.Collections.Generic;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.New
{
    public interface IUpdateCatalog : IReadOnlyCollection<IUpdateItem>
    {
        IReadOnlyCollection<IUpdateItem> ItemsToDownload { get; }

        IReadOnlyCollection<IUpdateItem> ItemsToDelete { get; }

        IReadOnlyCollection<IUpdateItem> ItemsToKeep { get; }
    }
}