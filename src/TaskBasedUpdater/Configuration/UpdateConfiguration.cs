namespace TaskBasedUpdater.Configuration
{
    public record UpdateConfiguration
    {
        public string? BackupPath { get; init; }

        public BackupPolicy BackupPolicy { get; init; }

        public int ConcurrentDownloads { get; init; } = 2;

        public bool DiagnosticMode { get; init; }

        public ValidationPolicy ValidationPolicy { get; init; }

        public bool DownloadOnlyMode { get; init; }

        public string? AlternativeDownloadPath { get; init; }
        
        public bool SupportsRestart { get; init; }

        public string? ExternalUpdaterPath { get; init; }
        
        public string? ExternalElevatorPath { get; init; }

        public bool RequiredElevationCancelsUpdate { get; init; }
    }
    
    public record DownloadManagerConfiguration
    {
        public static DownloadManagerConfiguration Default = new();
        
        public int DownloadRetryCount { get; init; } = 3;

        public int DownloadRetryDelay { get; init; } = 5000;

        public bool AllowEmptyFileDownload { get; init; }
    }
}