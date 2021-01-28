using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SimpleDownloadManager.Engines;
using SimpleDownloadManager.Verification;

namespace SimpleDownloadManager
{
    public interface IDownloadManager
    {
        IEnumerable<string> DefaultEngines { get; set; }

        IEnumerable<string> AllEngines { get; }

        void AddDownloadEngine(IDownloadEngine engine);

        Task<DownloadSummary> DownloadAsync(Uri uri, Stream outputStream, ProgressUpdateCallback? progress,
            CancellationToken cancellationToken, VerificationContext? verificationContext = null);
    }
}