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

        public void Add(IUpdateItem component, string downloadPath)
        {
            if (_downloadLookup.ContainsKey(component))
                _downloadLookup[component] = downloadPath;
            else
                _downloadLookup.Add(component, downloadPath);
        }

        public bool TryGetValue(IUpdateItem component, out string value)
        {
            return _downloadLookup.TryGetValue(component, out value);
        }

        public bool Remove(IUpdateItem component)
        {
            return _downloadLookup.Remove(component);
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
