namespace SimpleDownloadManager
{
    public class ProgressUpdateStatus
    {
        public long BytesRead { get; }

        public long TotalBytes { get; }

        public double BitRate { get; }

        public string? DownloadEngine { get; }

        public ProgressUpdateStatus(long bytesRead, long totalBytes, double bitRate)
            : this(null, bytesRead, totalBytes, bitRate)
        {
        }

        public ProgressUpdateStatus(string? downloadEngine, long bytesRead, long totalBytes, double bitRate)
        {
            DownloadEngine = downloadEngine;
            BytesRead = bytesRead;
            TotalBytes = totalBytes;
            BitRate = bitRate;
        }
    }
}