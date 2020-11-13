using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimplePipeline.Tasks;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Download;
using TaskBasedUpdater.Elevation;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.Tasks
{
    internal sealed class UpdateItemDownloadTask : SynchronizedPipelineTask, IUpdaterTask
    {
        public const string NewFileExtension = ".new";
        internal static readonly long AdditionalSizeBuffer = 20000000;

        private readonly ProgressUpdateCallback _progress;

        public Uri FailedDownloadUri { get; set; }

        public Uri Uri { get; }

        public IUpdateItem UpdateItem { get; }

        public string DownloadPath { get; private set; }

        public UpdateItemDownloadTask(IServiceProvider serviceProvider, IUpdateItem updateItem) 
            : base(serviceProvider)
        {
            UpdateItem = updateItem ?? throw new ArgumentNullException(nameof(updateItem));
            if (updateItem.OriginInfo?.Origin == null)
                throw new ArgumentNullException(nameof(OriginInfo));
            Uri = updateItem.OriginInfo.Origin;
        }

        public override string ToString()
        {
            return $"Downloading component '{UpdateItem.Name}' form \"{Uri}\"";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            var destination = UpdateItem.Destination;

            if (!Path.IsPathRooted(destination))
            {
                var directoryName = Path.GetDirectoryName(destination);
                if (string.IsNullOrEmpty(directoryName))
                    throw new InvalidOperationException("Unable to determine a download directory");
                Directory.CreateDirectory(directoryName);
            }

            if (UpdateConfiguration.Instance.BackupPolicy != BackupPolicy.NotRequired && UpdateConfiguration.Instance.DownloadOnlyMode)
                BackupComponent();


            Exception lastException = null;
            if (!token.IsCancellationRequested)
                DownloadAction(token, out lastException);

            token.ThrowIfCancellationRequested();

            if (lastException != null)
            {
                var action = lastException is ValidationFailedException ? "validate download" : "download";
                Logger.LogError(lastException, $"Failed to {action} from '{Uri}'. {lastException.Message}");
                throw lastException;
            }
        }

        private void BackupComponent()
        {
            try
            {
                BackupManager.Instance.CreateBackup(UpdateItem);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"Creating backup of {UpdateItem.Name} failed.");
                if (UpdateConfiguration.Instance.BackupPolicy == BackupPolicy.Required)
                {
                    Logger.LogTrace("Cancelling update operation due to BackupPolicy");
                    throw;
                }
            }
        }

        private void DownloadAction(CancellationToken token, out Exception? lastException)
        {
            lastException = null;
            var downloadManager = DownloadManager.Instance;

            for (var i = 0; i <= UpdateConfiguration.Instance.DownloadRetryCount; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                try
                {
                    var downloadPath = CalculateDownloadPath();
                    DownloadPath = downloadPath;
                    UpdateItemDownloadPathStorage.Instance.Add(UpdateItem, DownloadPath);

                    DownloadAndVerifyAsync(downloadManager, DownloadPath, token).Wait();
                    if (!File.Exists(DownloadPath))
                    {
                        var message = "File not found after being successfully downloaded and verified: " +
                                      DownloadPath + ", package: " + UpdateItem.Name;
                        Logger.LogWarning(message);
                        throw new FileNotFoundException(message, DownloadPath);
                    }

                    lastException = null;

                    if (UpdateConfiguration.Instance.DownloadOnlyMode)
                    {
                        UpdateItem.CurrentState = CurrentState.Installed;
                        UpdateItemDownloadPathStorage.Instance.Remove(UpdateItem);
                    }
                    else
                        UpdateItem.CurrentState = CurrentState.Downloaded;

                    break;
                }
                catch (OperationCanceledException ex)
                {
                    lastException = ex;
                    Logger.LogWarning($"Download of {Uri} was cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException && ex.IsExceptionType<OperationCanceledException>())
                    {
                        lastException = ex;
                        Logger.LogWarning($"Download of {Uri} was cancelled.");
                        break;
                    }
                    var wrappedException = ex.TryGetWrappedException();
                    if (wrappedException != null)
                        ex = wrappedException;
                    if (ex is UnauthorizedAccessException unauthorizedAccessException)
                    {
                        lastException = ex;
                        Logger.LogError(ex, $"Failed to download \"{Uri}\" to {DownloadPath}: {ex.Message}");
                        Elevator.Instance.RequestElevation(unauthorizedAccessException, UpdateItem);
                        break;
                    }
                    lastException = ex;
                    Logger.LogError(ex, $"Failed to download \"{Uri}\" on try {i}: {ex.Message}");
                }
            }
        }

        private string CalculateDownloadPath()
        {
            if (UpdateConfiguration.Instance.DownloadOnlyMode)
            {
                var destination = UpdateItem.GetFilePath(); 
                return destination;
            }

            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var backupFileName = $"{UpdateItem.Name}.{randomFileName}{NewFileExtension}";

            if (!string.IsNullOrEmpty(UpdateConfiguration.Instance.AlternativeDownloadPath))
            {
                Directory.CreateDirectory(UpdateConfiguration.Instance.AlternativeDownloadPath);
                return Path.Combine(UpdateConfiguration.Instance.AlternativeDownloadPath, backupFileName);
            }
            return Path.Combine(UpdateItem.Destination, backupFileName);
        }

        private async Task DownloadAndVerifyAsync(IDownloadManager downloadManager, string destination, CancellationToken token)
        {
            try
            {
                using var file = new FileStream(destination, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                await downloadManager.DownloadAsync(Uri, file, status => _progress?.Invoke(status), token, UpdateItem, true);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    Logger.LogTrace($"Deleting potentially partially downloaded file '{destination}' generated as a result of operation cancellation.");
                    File.Delete(destination);
                }
                catch (Exception e)
                {
                    Logger.LogTrace($"Could not delete partially downloaded file '{destination}' due to exception: {e}");
                }
                throw;
            }
        }

        // TODO: This has to be a precheck, because of parallel download tasks
        private static void ValidateEnoughDiskSpaceAvailable(IUpdateItem updateItem)
        {
            var option = DiskSpaceCalculator.CalculationOption.Download;
            if (UpdateConfiguration.Instance.DownloadOnlyMode)
            {
                option |= DiskSpaceCalculator.CalculationOption.Install;
                if (UpdateConfiguration.Instance.BackupPolicy != BackupPolicy.Disable)
                    option |= DiskSpaceCalculator.CalculationOption.Backup;
            }

            DiskSpaceCalculator.ThrowIfNotEnoughDiskSpaceAvailable(updateItem, AdditionalSizeBuffer, option);
        }
    }
}