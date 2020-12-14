using System;
using System.IO;
using System.Linq;
using System.Threading;
using TaskBasedUpdater.Component;

namespace TaskBasedUpdater.Download
{
    public abstract class DownloadEngineBase : IDownloadEngine, IDisposable
    {
        private readonly DownloadSource[] _supportedSources;

        public string Name { get; }

        protected DownloadEngineBase(string name, DownloadSource[] supportedSources)
        {
            Name = name;
            _supportedSources = supportedSources;
        }

        ~DownloadEngineBase()
        {
            Dispose(false);
        }

        public bool IsSupported(DownloadSource source)
        {
            return _supportedSources.Contains(source);
        }

        public DownloadSummary Download(Uri uri, Stream outputStream, ProgressUpdateCallback progress,
            CancellationToken cancellationToken, ProductComponent? productComponent)
        {
            return DownloadWithBitRate(uri, outputStream, progress, cancellationToken, productComponent);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract DownloadSummary DownloadCore(Uri uri, Stream outputStream, ProgressUpdateCallback progress,
            CancellationToken cancellationToken, ProductComponent? productComponent);

        protected virtual void DisposeResources()
        {
        }

        private DownloadSummary DownloadWithBitRate(Uri uri, Stream outputStream, ProgressUpdateCallback progress, CancellationToken cancellationToken, ProductComponent? productComponent)
        {
            var now = DateTime.Now;
            var lastProgressUpdate = now;
            ProgressUpdateCallback wrappedProgress = null;
            if (progress != null)
                wrappedProgress = p =>
                {
                    var now2 = DateTime.Now;
                    var timeSpan = now2 - lastProgressUpdate;
                    var bitRate = 8.0 * p.BytesRead / timeSpan.TotalSeconds;
                    progress(new ProgressUpdateStatus(p.BytesRead, p.TotalBytes, bitRate));
                    lastProgressUpdate = now2;
                };
            var downloadSummary = DownloadCore(uri, outputStream, wrappedProgress, cancellationToken, productComponent);
            downloadSummary.DownloadTime = DateTime.Now - now;
            downloadSummary.BitRate = 8.0 * downloadSummary.DownloadedSize / downloadSummary.DownloadTime.TotalSeconds;
            return downloadSummary;
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            DisposeResources();
        }
    }
}