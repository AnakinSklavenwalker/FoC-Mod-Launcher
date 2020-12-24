using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TaskBasedUpdater.Download
{
    internal class PreferredDownloadEngines
    {
        private static readonly object SyncRoot = new object();
        private static PreferredDownloadEngines? _instance;
        private static readonly ConcurrentDictionary<string, int> ConcurrentPreferredEngines = new();
        private readonly ConcurrentDictionary<string, int> _preferredEngines;
        private string? _lastSuccessfulEngineName;

        public static PreferredDownloadEngines Instance
        {
            get
            {
                if (_instance != null) 
                    return _instance;
                lock (SyncRoot) 
                    _instance ??= new PreferredDownloadEngines();
                return _instance;
            }
        }

        public string? LastSuccessfulEngineName
        {
            get => _lastSuccessfulEngineName;
            set
            {
                if (value == null)
                    return;
                _lastSuccessfulEngineName = value;
                _preferredEngines.AddOrUpdate(value, 1, (key, existingVal) =>
                {
                    ++existingVal;
                    return existingVal;
                });
            }
        }

        public PreferredDownloadEngines(ConcurrentDictionary<string, int>? preferredEngines = null)
        { 
            _preferredEngines = preferredEngines ?? ConcurrentPreferredEngines;
        }

        public IEnumerable<IDownloadEngine> GetEnginesInPriorityOrder(IEnumerable<IDownloadEngine> viableEngines)
        {
            lock (SyncRoot)
            {
                var downloadEngineList = _preferredEngines.OrderByDescending(i => i.Value)
                    .Select(downloadEngineNameAndFrequency => viableEngines.FirstOrDefault(e =>
                        string.Equals(e.Name, downloadEngineNameAndFrequency.Key, StringComparison.OrdinalIgnoreCase)))
                    .Where(downloadEngine => downloadEngine != null).ToList();
                foreach (var viableEngine in viableEngines)
                {
                    if (!downloadEngineList.Contains(viableEngine))
                        downloadEngineList.Add(viableEngine);
                }
                return downloadEngineList;
            }
        }
    }
}