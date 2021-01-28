using System;
using SimpleDownloadManager.Verification;

namespace SimpleDownloadManager
{
    public record DownloadSummary
    {
        public long DownloadedSize { get; init; }

        public double BitRate { get; init; }

        public TimeSpan DownloadTime { get; init; }

        public string DownloadEngine { get; init; }

        public ProxyResolution? ProxyResolution { get; internal set; }

        public string? FinalUri { get; internal set; }

        public VerificationResult ValidationResult { get; internal set; }

        public DownloadSummary()
            : this(null!, 0L, 0.0, TimeSpan.Zero, default)
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