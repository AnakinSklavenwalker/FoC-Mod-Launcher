namespace TaskBasedUpdater.Configuration
{
    public interface IUpdateConfiguration
    {
        string? BackupPath { get; }
        BackupPolicy BackupPolicy { get; }
        int ConcurrentDownloads { get; }
        bool DiagnosticMode { get; }
        int DownloadRetryDelay { get; }
        bool AllowEmptyFileDownload { get; }
        ValidationPolicy ValidationPolicy { get; }
        bool DownloadOnlyMode { get; }
        string? AlternativeDownloadPath { get; }
        int DownloadRetryCount { get; }
        bool SupportsRestart { get; }
        string? ExternalUpdaterPath { get; }
        string? ExternalElevatorPath { get; }
        bool RequiredElevationCancelsUpdate { get; }
    }
}