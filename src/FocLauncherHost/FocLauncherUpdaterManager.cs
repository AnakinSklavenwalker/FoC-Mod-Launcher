using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher;
using FocLauncher.Properties;
using FocLauncher.Shared;
using FocLauncher.Utilities;
using FocLauncherHost.Utilities;
using Newtonsoft.Json;
using TaskBasedUpdater;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Restart;
using ApplicationType = FocLauncher.ApplicationType;

namespace FocLauncherHost
{
    /*
    internal class FocLauncherUpdaterManager : UpdateManager
    {
        protected override IEnumerable<string> FileDeleteIgnoreFilter => new List<string> {".Theme.dll"};

        public FocLauncherInformation LauncherInformation { get; }

        public FocLauncherUpdaterManager(string versionMetadataPath) : base(LauncherConstants.ApplicationBasePath,
            versionMetadataPath)
        {
            SetUpdateConfiguration();
            LauncherInformation = FocLauncherInformation.Instance;
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
            return !UpdateItems.Any(x =>
                file.Name.Equals(x.Name) && x.Destination.Equals(LauncherConstants.ApplicationBasePath));
        }

        protected override async Task<IEnumerable<IUpdateItem>?> GetCatalogComponentsAsync(Stream catalogStream, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var products = await Catalogs.TryDeserializeAsync(catalogStream);
            if (products is null)
                throw new UpdaterException("Failed to deserialize metadata stream. Incompatible metadata version?");
            return GetCatalogComponents(products, catalogs => catalogs.FirstOrDefault(x => x.ApplicationType == ApplicationType.Stable));
        }

        protected IEnumerable<IUpdateItem>? GetCatalogComponents(Catalogs catalogs, Func<IEnumerable<ProductCatalog>, ProductCatalog> fallbackAction)
        {
            try
            {
                var product = FindMatchingProductCatalog(catalogs, fallbackAction);
                if (product == null)
                    return null;

                var result = new HashSet<IUpdateItem>(UpdateItemIdentityComparer.Default);
                foreach (var component in product.Dependencies.Select(CatalogExtensions.DependencyToComponent)
                    .Where(component => component != null))
                    result.Add(component);
                return result;
            }
            catch (Exception e)
            {
                throw new UpdaterException($"Failed to get update information from metadata: {e.Message}", e);
            }
        }

        protected override async Task<bool> ValidateCatalogStreamAsync(Stream inputStream)
        {
            var schemeStream = Resources.UpdateValidator.ToStream();
            var validator = new XmlValidator(schemeStream);
            return await Task.FromResult(validator.Validate(inputStream));
        }

        protected override IRestartOptions CreateRestartOptions(IReadOnlyCollection<IUpdateItem>? components = null)
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

            
            var args = options.ToCommandLine();
            Logger.Debug($"Created restart options: {args}");
            return options;
        }
        
        protected override bool PermitElevationRequest()
        {
            return LauncherRestartManager.ShowElevateDialog();
        }
        
        protected override async Task<PendingHandledResult> HandleLockedComponentsCoreAsync(
            ICollection<IUpdateItem> pendingComponents, ILockingProcessManager lockingProcessManager,
            bool ignoreSelfLocked, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!pendingComponents.Any())
                return new PendingHandledResult(HandlePendingItemStatus.Handled);

            Logger.Trace("Hanlde restart request due to locked files");

            var processes = lockingProcessManager.GetProcesses().ToList();
            var isSelfLocking = lockingProcessManager.ProcessesContainsSelf();

            if (!isSelfLocking && processes.Any(x => x.ApplicationType == TaskBasedUpdater.Restart.ApplicationType.Critical))
                return new PendingHandledResult(HandlePendingItemStatus.Declined,
                    "Files are locked by a system process that cannot be terminated. Please restart the system");

            using var lockingProcessManagerWithoutSelf = CreateFromProcessesWithoutSelf(processes);

            if (!isSelfLocking || ignoreSelfLocked)
            {
                var restartRequestResult =
                    LauncherRestartManager.ShowProcessKillDialog(lockingProcessManagerWithoutSelf, token);
                Logger.Trace($"Kill locking processes: {restartRequestResult}, Launcher needs restart: {false}");
                if (!restartRequestResult)
                    return new PendingHandledResult(HandlePendingItemStatus.Declined,
                        "Update aborted because locked files have not been released.");

                lockingProcessManagerWithoutSelf.Shutdown();
                LockedFilesWatcher.Instance.LockedFiles.Clear();
                await UpdateAsync(pendingComponents, token).ConfigureAwait(false);

                return LockedFilesWatcher.Instance.LockedFiles.Any()
                    ? new PendingHandledResult(HandlePendingItemStatus.HandledButStillPending,
                        "Update failed because there are still locked files which have not been released.")
                    : new PendingHandledResult(HandlePendingItemStatus.Handled);
            }

            if (!UpdateConfiguration.Instance.SupportsRestart)
                return new PendingHandledResult(HandlePendingItemStatus.Declined,
                    "Update requires a self-update which is not supported for this update configuration.");

            var result = LauncherRestartManager.ShowSelfKillDialog(lockingProcessManager, token);
            Logger.Trace($"Kill locking processes: {result}, Launcher needs restart: {true}");
            if (!result)
                return new PendingHandledResult(HandlePendingItemStatus.Declined,
                    "Update aborted because locked files have not been released.");

            lockingProcessManagerWithoutSelf.Shutdown();
            return new PendingHandledResult(HandlePendingItemStatus.Restart);
        }
        
        protected override Version? GetComponentVersion(IUpdateItem component)
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

        protected override void OnRestoreFailed(Exception ex, UpdateResultInformation updateInformation)
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
                LauncherRegistryHelper.WriteValue(LauncherRegistryKeys.ForceRestore, true);
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

        private ProductCatalog? FindMatchingProductCatalog(Catalogs catalogs, Func<IEnumerable<ProductCatalog>, ProductCatalog> fallbackAction)
        {
            if (catalogs?.Products is null || !catalogs.Products.Any())
                throw new NotSupportedException("No products to update are found");

            var productsWithCorrectName = catalogs.Products.Where(x =>
                    x.Name.Equals(LauncherInformation.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();

            var searchOption = LauncherInformation.CurrentUpdateSearchOption ?? LauncherInformation.UpdateSearchOption;

            var matchingProduct = productsWithCorrectName.FirstOrDefault(x => x.ApplicationType == searchOption);
            if (fallbackAction == null || matchingProduct != null || LauncherInformation.UpdateMode == UpdateMode.Explicit || LauncherInformation.UpdateMode == UpdateMode.NoFallback)
                return matchingProduct;

            var useFallback = true;
            if (LauncherInformation.UpdateMode == UpdateMode.AskFallbackStable)
            {
                var messageBoxResult = MessageBox.Show(
                    $"No updates for {searchOption}-Version available. Do you want to update to the latest stable version instead?",
                    "FoC Launcher", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                useFallback = messageBoxResult == MessageBoxResult.Yes;
            }
            return useFallback ? fallbackAction(productsWithCorrectName) : null;
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

        private IEnumerable<LauncherUpdaterItem> GetUpdaterItems(IEnumerable<IUpdateItem> components)
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

            var completedComponents = AllUpdateItems.Except(pendingComponents).ToList();
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
    */
}