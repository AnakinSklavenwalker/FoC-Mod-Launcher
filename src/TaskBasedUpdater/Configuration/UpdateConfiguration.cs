namespace TaskBasedUpdater.Configuration
{
    public class UpdateConfiguration : IUpdateConfiguration
    {
        public string? BackupPath { get; set; }

        public BackupPolicy BackupPolicy { get; set; }

        public int ConcurrentDownloads { get; set; } = 2;

        public bool DiagnosticMode { get; set; }

        public int DownloadRetryDelay { get; set; } = 5000;

        public bool AllowEmptyFileDownload { get; set; }

        public ValidationPolicy ValidationPolicy { get; set; }

        public bool DownloadOnlyMode { get; set; }

        public string? AlternativeDownloadPath { get; set; }

        public int DownloadRetryCount { get; set; } = 3;

        public bool SupportsRestart { get; set; }

        public string? ExternalUpdaterPath { get; set; }
        
        public string? ExternalElevatorPath { get; set; }

        public bool RequiredElevationCancelsUpdate { get; set; }
    }
}
