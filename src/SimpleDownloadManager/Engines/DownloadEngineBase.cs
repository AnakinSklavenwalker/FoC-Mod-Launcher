using System;
using System.IO;
using System.Linq;
using System.Threading;
using CommonUtilities;

namespace SimpleDownloadManager.Engines
{
    public abstract class DownloadEngineBase : DisposableObject, IDownloadEngine
    {
        private readonly DownloadSource[] _supportedSources;

        public string Name { get; }

        protected DownloadEngineBase(string name, DownloadSource[] supportedSources)
        {
            Name = name;
            _supportedSources = supportedSources;
        }
        
        public bool IsSupported(DownloadSource source)
        {
            return _supportedSources.Contains(source);
        }

        public DownloadSummary Download(Uri uri, Stream outputStream, ProgressUpdateCallback? progress,
            CancellationToken cancellationToken)
        {
            return DownloadWithBitRate(uri, outputStream, progress, cancellationToken);
        }

        protected abstract DownloadSummary DownloadCore(Uri uri, Stream outputStream, ProgressUpdateCallback? progress,
            CancellationToken cancellationToken);

        private DownloadSummary DownloadWithBitRate(Uri uri, Stream outputStream, ProgressUpdateCallback? progress, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var lastProgressUpdate = now;
            ProgressUpdateCallback? wrappedProgress = null;
            if (progress != null)
                wrappedProgress = p =>
                {
                    var now2 = DateTime.Now;
                    var timeSpan = now2 - lastProgressUpdate;
                    var bitRate = 8.0 * p.BytesRead / timeSpan.TotalSeconds;
                    progress(new ProgressUpdateStatus(p.BytesRead, p.TotalBytes, bitRate));
                    lastProgressUpdate = now2;
                };
            var downloadSummary = DownloadCore(uri, outputStream, wrappedProgress, cancellationToken);

            return downloadSummary with
            {
                DownloadTime = DateTime.Now - now,
                BitRate = 8.0 * downloadSummary.DownloadedSize / downloadSummary.DownloadTime.TotalSeconds
            };
        }
    }
}