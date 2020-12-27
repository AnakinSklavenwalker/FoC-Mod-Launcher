namespace TaskBasedUpdater.Configuration
{
    public record DownloadManagerConfiguration
    {
        public static DownloadManagerConfiguration Default = new();

        public int DownloadRetryDelay { get; init; } = 5000;

        public bool AllowEmptyFileDownload { get; init; }

        public ValidationPolicy ValidationPolicy { get; init; }
    }
}