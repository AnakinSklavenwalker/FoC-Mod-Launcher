using System;
using System.IO;
using System.Threading;
using TaskBasedUpdater.ProductComponent;

namespace TaskBasedUpdater.Download
{
    internal interface IDownloadEngine
    {
        string Name { get; }

        bool IsSupported(DownloadSource source);

        DownloadSummary Download(Uri uri, Stream outputStream, ProgressUpdateCallback progress, 
            CancellationToken cancellationToken, IUpdateItem? updateItem);
    }
}