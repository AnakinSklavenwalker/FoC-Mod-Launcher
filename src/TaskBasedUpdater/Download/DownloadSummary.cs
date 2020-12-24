using System;
using TaskBasedUpdater.Verification;

namespace TaskBasedUpdater.Download
{
    public class DownloadSummary
    {
        public long DownloadedSize { get; internal set; }

        public double BitRate { get; internal set; }

        public TimeSpan DownloadTime { get; internal set; }

        public string DownloadEngine { get; internal set; }

        public string? ProxyResolution { get; internal set; }

        public string? FinalUri { get; internal set; }

        public DownloadResult Result { get; internal set; }

        public VerificationResult ValidationResult { get; internal set; }

        public DownloadSummary()
            : this(null, 0L, 0.0, TimeSpan.Zero, default)
        {
        }

        public DownloadSummary(long downloadSize, double bitRate, TimeSpan downloadTime)
            : this(null, downloadSize, bitRate, downloadTime, default)
        {
        }

        public DownloadSummary(
            string downloadEngine,
            long downloadSize,
            double bitRate,
            TimeSpan downloadTime)
            : this(downloadEngine, downloadSize, bitRate, downloadTime, default)
        {
        }

        public DownloadSummary(string downloadEngine, long downloadSize, double bitRate, TimeSpan downloadTime, VerificationResult validationResult)
        {
            DownloadEngine = downloadEngine;
            DownloadedSize = downloadSize;
            BitRate = bitRate;
            DownloadTime = downloadTime;
            ProxyResolution = null;
            FinalUri = null;
            ValidationResult = validationResult;
        }
    }
}