using System;
using System.Collections.Generic;

namespace ProductUpdater.Restart
{
    public class LockedFilesWatcher
    {
        private static LockedFilesWatcher? _instance;

        private readonly HashSet<string> _lockedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static LockedFilesWatcher Instance => _instance ??= new LockedFilesWatcher();

        public ISet<string> LockedFiles => _lockedFiles;

        private LockedFilesWatcher()
        {
        }

        public void Clear()
        {
            _lockedFiles.Clear();
        }
    }
}