namespace TaskBasedUpdater.Configuration
{
    public record UpdateConfiguration
    {
        public DownloadManagerConfiguration DownloadConfiguration { get; init; } = new();

        public string? BackupPath { get; init; }

        public BackupPolicy BackupPolicy { get; init; }

        public int ConcurrentDownloads { get; init; } = 2;

        public bool DiagnosticMode { get; init; }
        
        public InstallMode InstallMode { get; init; }

        public int DownloadRetryCount { get; init; } = 3;

        public string? AlternativeDownloadPath { get; init; }
        
        public bool SupportsRestart { get; init; }

        public string? ExternalUpdaterPath { get; init; }
        
        public string? ExternalElevatorPath { get; init; }

        public bool RequiredElevationCancelsUpdate { get; init; }
    }
}