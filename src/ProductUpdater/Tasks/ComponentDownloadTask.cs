using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductMetadata.Component;
using ProductUpdater.Configuration;
using ProductUpdater.Elevation;
using SimpleDownloadManager;
using SimpleDownloadManager.Verification;
using SimplePipeline.Tasks;
using Validation;

namespace ProductUpdater.Tasks
{
    internal sealed class ComponentDownloadTask : SynchronizedPipelineTask, IComponentTask
    {
        public const string NewFileExtension = ".new";
        internal static readonly long AdditionalSizeBuffer = 20000000;

        // TODO: Progress
        private readonly ProgressUpdateCallback? _progress;

        public Uri Uri { get; }

        public ProductComponent ProductComponent { get; }

        public string DownloadPath { get; private set; }

        public UpdateConfiguration Configuration { get; }

        public ComponentDownloadTask(ProductComponent productComponent, UpdateConfiguration configuration, ILogger? logger = null) 
            : base(logger)
        {
            Requires.NotNull(productComponent, nameof(productComponent));
            Requires.NotNull(configuration, nameof(configuration));
            ProductComponent = productComponent;
            Configuration = configuration;
            if (productComponent.OriginInfo?.Origin == null)
                throw new ArgumentNullException(nameof(OriginInfo));
            Uri = productComponent.OriginInfo.Origin;
        }

        public override string ToString()
        {
            return $"Downloading item '{ProductComponent.Name}' form \"{Uri}\"";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            var destination = ProductComponent.Destination;

            if (!Path.IsPathRooted(destination))
            {
                var directoryName = Path.GetDirectoryName(destination);
                if (string.IsNullOrEmpty(directoryName))
                    throw new InvalidOperationException("Unable to determine a download directory");
                Directory.CreateDirectory(directoryName);
            }

            if (Configuration.BackupPolicy != BackupPolicy.NotRequired && Configuration.InstallMode == InstallMode.DownloadOnly)
                BackupItem();


            Exception? lastException = null;
            if (!token.IsCancellationRequested)
                DownloadAction(token, out lastException);

            token.ThrowIfCancellationRequested();

            if (lastException != null)
            {
                var action = lastException is VerificationFailedException ? "validate download" : "download";
                Logger.LogError(lastException, $"Failed to {action} from '{Uri}'. {lastException.Message}");
                throw lastException;
            }
        }

        private void BackupItem()
        {
            try
            {
                BackupManager.Instance.CreateBackup(ProductComponent);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"Creating backup of {ProductComponent.Name} failed.");
                if (Configuration.BackupPolicy == BackupPolicy.Required)
                {
                    Logger.LogTrace("Cancelling update operation due to BackupPolicy");
                    throw;
                }
            }
        }

        private void DownloadAction(CancellationToken token, out Exception? lastException)
        {
            lastException = null;
            // TODO: split-projects use 2 param ctor
            var downloadManager = new DownloadManager(Configuration.DownloadConfiguration);
            for (var i = 0; i <= Configuration.DownloadRetryCount; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                try
                {
                    var downloadPath = CalculateDownloadPath();
                    DownloadPath = downloadPath;
                    UpdateItemDownloadPathStorage.Instance.Add(ProductComponent, DownloadPath);

                    DownloadAndVerifyAsync(downloadManager, DownloadPath, token).Wait();
                    if (!File.Exists(DownloadPath))
                    {
                        var message = "File not found after being successfully downloaded and verified: " +
                                      DownloadPath + ", package: " + ProductComponent.Name;
                        Logger.LogWarning(message);
                        throw new FileNotFoundException(message, DownloadPath);
                    }

                    lastException = null;

                    if (Configuration.InstallMode == InstallMode.DownloadOnly)
                    {
                        // TODO: split-projects
                        //ProductComponent.CurrentState = CurrentState.Installed;
                        UpdateItemDownloadPathStorage.Instance.Remove(ProductComponent);
                    }
                    else
                    {
                        // TODO: split-projects
                        // ProductComponent.CurrentState = CurrentState.Downloaded;
                    }

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
                        Elevator.Instance.RequestElevation(unauthorizedAccessException, ProductComponent);
                        break;
                    }
                    lastException = ex;
                    Logger.LogError(ex, $"Failed to download \"{Uri}\" on try {i}: {ex.Message}");
                }
            }
        }

        private string CalculateDownloadPath()
        {
            if (Configuration.InstallMode == InstallMode.DownloadOnly)
            {
                var destination = ProductComponent.GetFilePath();
                return destination;
            }

            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var backupFileName = $"{ProductComponent.Name}.{randomFileName}{NewFileExtension}";

            var alternateDownloadPath = Configuration.AlternativeDownloadPath;
            if (!string.IsNullOrEmpty(alternateDownloadPath))
            {
                Directory.CreateDirectory(alternateDownloadPath!);
                return Path.Combine(alternateDownloadPath!, backupFileName);
            }
            return Path.Combine(ProductComponent.Destination, backupFileName);
        }

        private async Task DownloadAndVerifyAsync(IDownloadManager downloadManager, string destination, CancellationToken token)
        {
            try
            {
                var verificationContext = ToVerificationContext(ProductComponent.OriginInfo!.IntegrityInformation);
                using var file = new FileStream(destination, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                await downloadManager.DownloadAsync(Uri, file, status => _progress?.Invoke(status), token, verificationContext);
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
        private static void ValidateEnoughDiskSpaceAvailable(ProductComponent productComponent)
        {
            //var option = DiskSpaceCalculator.CalculationOption.Download;
            //if (UpdateConfiguration.Instance.DownloadOnlyMode)
            //{
            //    option |= DiskSpaceCalculator.CalculationOption.Install;
            //    if (UpdateConfiguration.Instance.BackupPolicy != BackupPolicy.Disable)
            //        option |= DiskSpaceCalculator.CalculationOption.Backup;
            //}

            //DiskSpaceCalculator.ThrowIfNotEnoughDiskSpaceAvailable(updateItem, AdditionalSizeBuffer, option);
        }

        private static VerificationContext ToVerificationContext(ComponentIntegrityInformation integrityInformation)
        {
            return integrityInformation.Equals(ComponentIntegrityInformation.None)
                ? VerificationContext.None
                : new VerificationContext(integrityInformation.Hash, integrityInformation.HashType);
        }
    }
}