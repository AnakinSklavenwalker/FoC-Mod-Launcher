using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using SimplePipeline.Tasks;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.FileSystem;

namespace TaskBasedUpdater.Tasks
{
    internal class UpdateItemInstallTask : SynchronizedPipelineTask, IUpdaterTask
    {
        private readonly UpdateItemDownloadTask _download;
        internal static readonly long AdditionalSizeBuffer = 20000000;
        private readonly bool _isPresent;

        internal UpdateAction Action { get; }

        public IUpdateItem UpdateItem { get; }

        internal InstallResult Result { get; set; }

        internal bool? RestartRequired { get; private set; }

        public virtual TimeSpan DownloadWaitTime { get; internal set; } = new TimeSpan(0L);

        public UpdateItemInstallTask(IServiceProvider serviceProvider,
            IUpdateItem updateItem, UpdateAction action, UpdateItemDownloadTask download,
            bool isPresent = false) :
            this(serviceProvider, updateItem, action, isPresent)
        {
            _download = download;
        }

        public UpdateItemInstallTask(IServiceProvider serviceProvider, IUpdateItem updateItem, UpdateAction action,
            bool isPresent = false) : base(serviceProvider)
        {
            UpdateItem = updateItem ?? throw new ArgumentNullException(nameof(updateItem));
            Action = action;
            _isPresent = isPresent;
        }

        public override string ToString()
        {
            return $"{Action}ing \"{UpdateItem.Name}\"";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (Action == UpdateAction.Keep)
            {
                Result = InstallResult.Success;
                return;
            }

            var now = DateTime.Now;
            _download?.Wait();
            DownloadWaitTime += DateTime.Now - now;
            if (_download?.Error != null)
            {
                Logger.LogWarning($"Skipping {Action} of '{UpdateItem.Name}' since downloading it failed: {_download.Error.Message}");
                return;
            }

            var installer = FileInstaller.Instance;
            try
            {
                try
                {
                    ValidateEnoughDiskSpaceAvailable(UpdateItem);

                    // TODO: split-projects
                    //if (UpdateConfiguration.Instance.BackupPolicy != BackupPolicy.Disable)
                    //    BackupItem();

                    if (Action == UpdateAction.Update)
                    {
                        string localPath;
                        if (_download != null)
                            localPath = _download.DownloadPath;
                        else if (UpdateItem.CurrentState == CurrentState.Downloaded && UpdateItemDownloadPathStorage.Instance.TryGetValue(UpdateItem, out var downloadedFile))
                            localPath = downloadedFile;
                        else
                            throw new FileNotFoundException("Unable to find the downloaded file.");

                        Result = installer.Install(UpdateItem, token, localPath, _isPresent);
                    }
                    else if (Action == UpdateAction.Delete)
                    {
                        Result = installer.Remove(UpdateItem, token, _isPresent);
                    }

                }
                catch (OutOfDiskspaceException)
                {
                    Result = InstallResult.Failure;
                    throw;
                }

                if (Result == InstallResult.SuccessRestartRequired)
                {
                    RestartRequired = true;
                }

                if (Result.IsFailure())
                    throw new UpdateItemFailedException(new[] { UpdateItem });
                if (Result == InstallResult.Cancel)
                    throw new OperationCanceledException();
            }
            finally
            {
            }
        }

        private static void ValidateEnoughDiskSpaceAvailable(IUpdateItem updateItem)
        {
            if (updateItem.RequiredAction == UpdateAction.Keep)
                return;
            var option = DiskSpaceCalculator.CalculationOption.All;
            if (updateItem.CurrentState == CurrentState.Downloaded)
                option &= ~DiskSpaceCalculator.CalculationOption.Download;
            // TODO: split-projects
            //if (UpdateConfiguration.Instance.BackupPolicy == BackupPolicy.Disable)
            //    option &= ~DiskSpaceCalculator.CalculationOption.Backup;
            DiskSpaceCalculator.ThrowIfNotEnoughDiskSpaceAvailable(updateItem, AdditionalSizeBuffer, option);
        }

        private void BackupItem()
        {
            try
            {
                BackupManager.Instance.CreateBackup(UpdateItem);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"Creating backup of {UpdateItem.Name} failed.");
                // TODO: split-projects
                //if (UpdateConfiguration.Instance.BackupPolicy == BackupPolicy.Required)
                //{
                //    Logger.LogTrace("Cancelling update operation due to BackupPolicy");
                //    throw;
                //}
            }
        }
    }
}