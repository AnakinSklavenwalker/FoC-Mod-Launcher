using System.Collections;
using System.Collections.Generic;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.New
{
    public class UpdateCatalog : IUpdateCatalog
    {
        private readonly IReadOnlyCollection<IUpdateItem> _allItems;

        public int Count => _allItems.Count;
        public IReadOnlyCollection<IUpdateItem> ItemsToDownload { get; }
        public IReadOnlyCollection<IUpdateItem> ItemsToKeep { get; }
        public IReadOnlyCollection<IUpdateItem> ItemsToDelete { get; }

        public UpdateCatalog(IEnumerable<IUpdateItem>? itemsToKeep,
            IEnumerable<IUpdateItem>? itemsToDownload,
            IEnumerable<IUpdateItem>? itemsToDelete)
        {
            // TODO
        }

        public IEnumerator<IUpdateItem> GetEnumerator() => _allItems.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}