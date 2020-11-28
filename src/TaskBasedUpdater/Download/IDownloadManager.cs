using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedUpdater.Download
{
    public interface IDownloadManager
    {
        IEnumerable<string> DefaultEngines { get; set; }

        IEnumerable<string> AllEngines { get; }

        Task<DownloadSummary> DownloadAsync(Uri uri, Stream outputStream, ProgressUpdateCallback progress,
            CancellationToken cancellationToken, IUpdateItem? updateItem = null, bool verify = false);
    }
}