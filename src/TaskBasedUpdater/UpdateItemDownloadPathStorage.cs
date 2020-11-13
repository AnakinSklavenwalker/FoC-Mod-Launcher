using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater
{
    public class UpdateItemDownloadPathStorage : IEnumerable<KeyValuePair<IUpdateItem, string>>
    {
        private static UpdateItemDownloadPathStorage? _instance;

        private readonly IDictionary<IUpdateItem, string> _downloadLookup =
            new ConcurrentDictionary<IUpdateItem, string>();

        public static UpdateItemDownloadPathStorage Instance => _instance ??= new UpdateItemDownloadPathStorage();

        private UpdateItemDownloadPathStorage()
        {
        }

        public void Add(IUpdateItem item, string downloadPath)
        {
            if (_downloadLookup.ContainsKey(item))
                _downloadLookup[item] = downloadPath;
            else
                _downloadLookup.Add(item, downloadPath);
        }

        public bool TryGetValue(IUpdateItem item, out string value)
        {
            return _downloadLookup.TryGetValue(item, out value);
        }

        public bool Remove(IUpdateItem item)
        {
            return _downloadLookup.Remove(item);
        }

        public void Clear()
        {
            _downloadLookup.Clear();
        }

        public IEnumerator<KeyValuePair<IUpdateItem, string>> GetEnumerator()
        {
            return _downloadLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
