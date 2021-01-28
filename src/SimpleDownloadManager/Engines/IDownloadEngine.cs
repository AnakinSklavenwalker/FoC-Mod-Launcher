using System;
using System.IO;
using System.Threading;

namespace SimpleDownloadManager.Engines
{
    public interface IDownloadEngine
    {
        string Name { get; }

        bool IsSupported(DownloadSource source);

        DownloadSummary Download(Uri uri, Stream outputStream, ProgressUpdateCallback progress, 
            CancellationToken cancellationToken);
    }
}