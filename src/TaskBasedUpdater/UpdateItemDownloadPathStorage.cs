using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TaskBasedUpdater
{
    public class UpdateItemDownloadPathStorage : IEnumerable<KeyValuePair<ProductComponent, string>>
    {
        private static UpdateItemDownloadPathStorage? _instance;

        private readonly IDictionary<ProductComponent, string> _downloadLookup =
            new ConcurrentDictionary<ProductComponent, string>();

        public static UpdateItemDownloadPathStorage Instance => _instance ??= new UpdateItemDownloadPathStorage();

        private UpdateItemDownloadPathStorage()
        {
        }

        public void Add(ProductComponent component, string downloadPath)
        {
            if (_downloadLookup.ContainsKey(component))
                _downloadLookup[component] = downloadPath;
            else
                _downloadLookup.Add(component, downloadPath);
        }

        public bool TryGetValue(ProductComponent component, out string value)
        {
            return _downloadLookup.TryGetValue(component, out value);
        }

        public bool Remove(ProductComponent component)
        {
            return _downloadLookup.Remove(component);
        }

        public void Clear()
        {
            _downloadLookup.Clear();
        }

        public IEnumerator<KeyValuePair<ProductComponent, string>> GetEnumerator()
        {
            return _downloadLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
