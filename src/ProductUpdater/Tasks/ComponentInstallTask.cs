﻿using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using Microsoft.Extensions.Logging;
using ProductMetadata.Component;
using ProductUpdater.Configuration;
using ProductUpdater.Validation;
using SimplePipeline.Tasks;
using Validation;

namespace ProductUpdater.Tasks
{
    internal class ComponentInstallTask : SynchronizedPipelineTask, IComponentTask
    {
        private readonly ComponentDownloadTask? _download;
        internal static readonly long AdditionalSizeBuffer = 20000000;
        private readonly IFileSystem _fileSystem;
        private readonly bool _isPresent;

        internal ComponentAction Action { get; }

        public ProductComponent ProductComponent { get; }

        internal InstallResult Result { get; set; }

        internal bool? RestartRequired { get; private set; }

        public virtual TimeSpan DownloadWaitTime { get; internal set; } = new(0L);

        public UpdateConfiguration Configuration { get; }

        public ComponentInstallTask(
            IFileSystem fileSystem,
            ProductComponent productComponent, 
            ComponentAction action, 
            UpdateConfiguration configuration,
            ComponentDownloadTask download,
            bool isPresent = false, 
            ILogger? logger = null) :
            this(fileSystem, productComponent, action, configuration, isPresent)
        {
            _download = download;
        }

        public ComponentInstallTask(
            IFileSystem fileSystem,
            ProductComponent productComponent,
            ComponentAction action,
            UpdateConfiguration configuration,
            bool isPresent = false, 
            ILogger? logger = null) : base(logger)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNull(productComponent, nameof(productComponent));
            Requires.NotNull(configuration, nameof(configuration));
            ProductComponent = productComponent;
            Configuration = configuration;
            Action = action;
            _fileSystem = fileSystem;
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

                    if (Configuration.BackupPolicy != BackupPolicy.Disable)
                        BackupItem();

                    if (Action == ComponentAction.Update)
                    {
                        string localPath;
                        if (_download != null)
                            localPath = _download.DownloadPath;
                        // TODO: split-projects
                        //else if (ProductComponent.CurrentState == CurrentState.Downloaded && UpdateItemDownloadPathStorage.Instance.TryGetValue(ProductComponent, out var downloadedFile))
                        //    localPath = downloadedFile;
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
            // TODO: split-projects
            //if (component.CurrentState == CurrentState.Downloaded)
            //    option &= ~DiskSpaceCalculator.CalculationOption.Download;
            if (Configuration.BackupPolicy == BackupPolicy.Disable)
                option &= ~DiskSpaceCalculator.CalculationOption.Backup;

            foreach (var d in new DiskSpaceCalculator(_fileSystem, ProductComponent, AdditionalSizeBuffer, option).CalculatedDiskSizes)
            {
                if (!d.Value.HasEnoughDiskSpace)
                    throw new OutOfDiskspaceException(
                        $"There is not enough space to install “{ProductComponent.Name}”.{d.Value.RequestedSize + AdditionalSizeBuffer} is required on drive {d.Key}" +
                        $"but you only have {d.Value.AvailableDiskSpace} available.");
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
    }
}