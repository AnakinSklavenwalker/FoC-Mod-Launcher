using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using SimplePipeline.Tasks;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.Validation;

namespace TaskBasedUpdater.Tasks
{
    internal class ComponentInstallTask : SynchronizedPipelineTask, IUpdaterTask
    {
        private readonly ComponentDownloadTask _download;
        internal static readonly long AdditionalSizeBuffer = 20000000;
        private readonly bool _isPresent;

        internal ComponentAction Action { get; }

        public ProductComponent ProductComponent { get; }

        internal InstallResult Result { get; set; }

        internal bool? RestartRequired { get; private set; }

        public virtual TimeSpan DownloadWaitTime { get; internal set; } = new TimeSpan(0L);

        public ComponentInstallTask(IServiceProvider serviceProvider,
            ProductComponent productComponent, ComponentAction action, ComponentDownloadTask download,
            bool isPresent = false) :
            this(serviceProvider, productComponent, action, isPresent)
        {
            _download = download;
        }

        public ComponentInstallTask(IServiceProvider serviceProvider, ProductComponent productComponent, ComponentAction action,
            bool isPresent = false) : base(serviceProvider)
        {
            ProductComponent = productComponent ?? throw new ArgumentNullException(nameof(productComponent));
            Action = action;
            _isPresent = isPresent;
        }

        public override string ToString()
        {
            return $"{Action}ing \"{ProductComponent.Name}\"";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (Action == ComponentAction.Keep)
            {
                Result = InstallResult.Success;
                return;
            }

            var now = DateTime.Now;
            _download?.Wait();
            DownloadWaitTime += DateTime.Now - now;
            if (_download?.Error != null)
            {
                Logger.LogWarning($"Skipping {Action} of '{ProductComponent.Name}' since downloading it failed: {_download.Error.Message}");
                return;
            }

            var installer = FileInstaller.Instance;
            try
            {
                try
                {
                    ValidateEnoughDiskSpaceAvailable(ProductComponent);

                    // TODO: split-projects
                    //if (UpdateConfiguration.Instance.BackupPolicy != BackupPolicy.Disable)
                    //    BackupItem();

                    if (Action == ComponentAction.Update)
                    {
                        string localPath;
                        if (_download != null)
                            localPath = _download.DownloadPath;
                        else if (ProductComponent.CurrentState == CurrentState.Downloaded && UpdateItemDownloadPathStorage.Instance.TryGetValue(ProductComponent, out var downloadedFile))
                            localPath = downloadedFile;
                        else
                            throw new FileNotFoundException("Unable to find the downloaded file.");

                        Result = installer.Install(ProductComponent, token, localPath, _isPresent);
                    }
                    else if (Action == ComponentAction.Delete)
                    {
                        Result = installer.Remove(ProductComponent, token, _isPresent);
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
                    throw new ComponentFailedException(new[] { ProductComponent });
                if (Result == InstallResult.Cancel)
                    throw new OperationCanceledException();
            }
            finally
            {
            }
        }

        private void ValidateEnoughDiskSpaceAvailable(ProductComponent component)
        {
            if (component.RequiredAction == ComponentAction.Keep)
                return;
            var option = DiskSpaceCalculator.CalculationOption.All;
            if (component.CurrentState == CurrentState.Downloaded)
                option &= ~DiskSpaceCalculator.CalculationOption.Download;
            // TODO: split-projects
            //if (UpdateConfiguration.Instance.BackupPolicy == BackupPolicy.Disable)
            //    option &= ~DiskSpaceCalculator.CalculationOption.Backup;
            DiskSpaceCalculator.ThrowIfNotEnoughDiskSpaceAvailable(ServiceProvider, component, AdditionalSizeBuffer, option);
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