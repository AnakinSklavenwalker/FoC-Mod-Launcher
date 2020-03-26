﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher;
using FocLauncher.Shared;
using FocLauncherHost.Properties;
using FocLauncherHost.Update.UpdateCatalog;
using FocLauncherHost.Utilities;
using Newtonsoft.Json;
using TaskBasedUpdater;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Restart;

namespace FocLauncherHost
{
    internal class FocLauncherUpdaterManager : UpdateManager
    {
        protected override IEnumerable<string> FileDeleteIgnoreFilter => new List<string> {".Theme.dll"};

        public FocLauncherUpdaterManager(string versionMetadataPath) : base(FocLauncherProduct.Instance,
            versionMetadataPath)
        {
            SetUpdateConfiguration();
        }

        private static void SetUpdateConfiguration()
        {
            UpdateConfiguration.Instance.BackupPolicy = BackupPolicy.Required;
            UpdateConfiguration.Instance.DownloadRetryCount = 1;
            UpdateConfiguration.Instance.BackupPath = Path.Combine(LauncherConstants.ApplicationBasePath, "Backups");
            UpdateConfiguration.Instance.DownloadRetryDelay = 500;
            UpdateConfiguration.Instance.SupportsRestart = true;
            UpdateConfiguration.Instance.ExternalUpdaterPath = LauncherConstants.UpdaterPath;
            UpdateConfiguration.Instance.RequiredElevationCancelsUpdate = true;
            UpdateConfiguration.Instance.AlternativeDownloadPath =
                Path.Combine(LauncherConstants.ApplicationBasePath, "Downloads");
        }

        protected override bool FileCanBeDeleted(FileInfo file)
        {
            return !Components.Any(x =>
                file.Name.Equals(x.Name) && x.Destination.Equals(LauncherConstants.ApplicationBasePath));
        }

        protected override async Task<IEnumerable<IComponent>> GetCatalogComponentsAsync(Stream catalogStream,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var products = await TryGetProductFromStreamAsync(catalogStream);
            if (products is null)
                throw new UpdaterException("Failed to deserialize metadata stream. Incompatible version?");

            var product = GetCatalog(products);
            if (product is null)
                throw new UpdaterException("No products to update are found");

            var result = new HashSet<IComponent>(ComponentIdentityComparer.Default);
            foreach (var component in product.Dependencies.Select(DependencyHelper.DependencyToComponent)
                .Where(component => component != null))
                result.Add(component);
            return result;
        }

        protected override async Task<bool> ValidateCatalogStreamAsync(Stream inputStream)
        {
            var schemeStream = Resources.UpdateValidator.ToStream();
            var validator = new XmlValidator(schemeStream);
            return await Task.FromResult(validator.Validate(inputStream));
        }

        protected override IRestartOptions CreateRestartOptions(IReadOnlyCollection<IComponent>? components = null)
        {
            var options = new LauncherRestartOptions
            {
                Pid = Process.GetCurrentProcess().Id,
                ExecutablePath = Environment.GetCommandLineArgs()[0],
                Update = components != null && components.Any(),
                LogFile = LauncherConstants.LogFilePath
            };

            if (options.Update && components != null)
            {
                var output = JsonConvert.SerializeObject(GetUpdaterItems(components));
                options.Payload = Base64Encode(output);
            }

            
            var args = options.Unparse();
            Logger.Debug($"Created restart options: {args}");
            return options;
        }
        
        protected override bool PermitElevationRequest()
        {
            return LauncherRestartManager.ShowElevateDialog();
        }
        
        protected override async Task<PendingHandledResult> HandleLockedComponentsCoreAsync(
            ICollection<IComponent> pendingComponents, ILockingProcessManager lockingProcessManager,
            bool ignoreSelfLocked, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!pendingComponents.Any())
                return new PendingHandledResult(HandlePendingComponentsStatus.Handled);

            Logger.Trace("Hanlde restart request due to locked files");

            var processes = lockingProcessManager.GetProcesses().ToList();
            var isSelfLocking = lockingProcessManager.ProcessesContainsSelf();

            if (!isSelfLocking && processes.Any(x => x.ApplicationType == ApplicationType.Critical))
                return new PendingHandledResult(HandlePendingComponentsStatus.Declined,
                    "Files are locked by a system process that cannot be terminated. Please restart the system");

            using var lockingProcessManagerWithoutSelf = CreateFromProcessesWithoutSelf(processes);

            if (!isSelfLocking || ignoreSelfLocked)
            {
                var restartRequestResult =
                    LauncherRestartManager.ShowProcessKillDialog(lockingProcessManagerWithoutSelf, token);
                Logger.Trace($"Kill locking processes: {restartRequestResult}, Launcher needs restart: {false}");
                if (!restartRequestResult)
                    return new PendingHandledResult(HandlePendingComponentsStatus.Declined,
                        "Update aborted because locked files have not been released.");

                lockingProcessManagerWithoutSelf.Shutdown();
                LockedFilesWatcher.Instance.LockedFiles.Clear();
                await UpdateAsync(pendingComponents, token).ConfigureAwait(false);

                return LockedFilesWatcher.Instance.LockedFiles.Any()
                    ? new PendingHandledResult(HandlePendingComponentsStatus.HandledButStillPending,
                        "Update failed because there are still locked files which have not been released.")
                    : new PendingHandledResult(HandlePendingComponentsStatus.Handled);
            }

            if (!UpdateConfiguration.Instance.SupportsRestart)
                return new PendingHandledResult(HandlePendingComponentsStatus.Declined,
                    "Update requires a self-update which is not supported for this update configuration.");

            var result = LauncherRestartManager.ShowSelfKillDialog(lockingProcessManager, token);
            Logger.Trace($"Kill locking processes: {result}, Launcher needs restart: {true}");
            if (!result)
                return new PendingHandledResult(HandlePendingComponentsStatus.Declined,
                    "Update aborted because locked files have not been released.");

            lockingProcessManagerWithoutSelf.Shutdown();
            return new PendingHandledResult(HandlePendingComponentsStatus.Restart);
        }
        
        protected override Version? GetComponentVersion(IComponent component)
        {
            try
            {
                return UpdaterUtilities.GetAssemblyFileVersion(component.GetFilePath());
            }
            catch
            {
                return null;
            }
        }

        protected override void OnRestoreFailed(Exception ex, UpdateInformation updateInformation)
        {
            base.OnRestoreFailed(ex, updateInformation);
            Clean().Wait();
            if (LauncherRestartManager.ShowRestoreDialog(true))
            {
                var options = CreateRestoreRestartOptions();
                if (options is null)
                    throw ex;
                ApplicationRestartManager.RestartApplication(options);
            }
            else
            {
                // TODO: Save in registry that a restore should be performed next start
                Environment.Exit(-1);
            }
               
        }

        private IRestartOptions? CreateRestoreRestartOptions()
        {
            if (!(CreateRestartOptions() is LauncherRestartOptions options))
                return null;
            options.Restore = true;
            return options;
        }

        private ProductCatalog GetCatalog(Catalogs catalogs)
        {
            if (catalogs?.Products is null || !catalogs.Products.Any())
                throw new NotSupportedException("No products to update are found");

            return catalogs.Products.FirstOrDefault(x =>
                x.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        private static Task<Catalogs> TryGetProductFromStreamAsync(Stream stream)
        {
            try
            {
                Logger.Trace("Try deserializing stream to Catalogs");
                return Catalogs.DeserializeAsync(stream);
            }
            catch (Exception e)
            {
                Logger.Debug(e, "Getting catalogs from stream failed with exception. Returning null instead.");
                return Task.FromResult<Catalogs>(null);
            }
        }

        private static ILockingProcessManager CreateFromProcessesWithoutSelf(IEnumerable<ILockingProcessInfo> processes)
        {
            var processesWithoutSelf = WithoutProcess(processes, Process.GetCurrentProcess().Id);
            var processManager = LockingProcessManagerFactory.Create();
            processManager.Register(null, processesWithoutSelf);
            return processManager;
        }

        private static IEnumerable<ILockingProcessInfo> WithoutProcess(IEnumerable<ILockingProcessInfo> processes, int processId)
        {
            return processes.Where(x => !x.Id.Equals(processId));
        }

        private IEnumerable<LauncherUpdaterItem> GetUpdaterItems(IEnumerable<IComponent> components)
        {
            if (components is null)
                throw new ArgumentNullException(nameof(components));

            var pendingComponents = components.ToList();
            foreach (var pendingComponent in pendingComponents)
            {
                var item = new LauncherUpdaterItem();
                var componentFilePath = pendingComponent.GetFilePath();
                switch (pendingComponent.RequiredAction)
                {
                    case ComponentAction.Keep:
                        continue;
                    case ComponentAction.Delete:
                        item.File = componentFilePath;
                        item.Destination = null;
                        break;
                    case ComponentAction.Update:
                        ComponentDownloadPathStorage.Instance.TryGetValue(pendingComponent, out var file);
                        item.File = file;
                        item.Destination = componentFilePath;
                        break;
                }
                item.BackupDestination = componentFilePath;
                BackupManager.Instance.TryGetValue(pendingComponent, out var backup);
                item.Backup = backup;
                yield return item;
            }

            var completedComponents = AllComponents.Except(pendingComponents).ToList();
            foreach (var completedComponent in completedComponents)
            {
                if (completedComponent.RequiredAction == ComponentAction.Keep)
                    continue;
                var item = new LauncherUpdaterItem {BackupDestination = completedComponent.GetFilePath()};
                BackupManager.Instance.TryGetValue(completedComponent, out var backup);
                if (!string.IsNullOrEmpty(backup))
                    item.Backup = backup;
                yield return item;
            }

        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}