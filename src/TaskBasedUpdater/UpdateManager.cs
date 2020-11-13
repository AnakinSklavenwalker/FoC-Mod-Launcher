using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Download;
using TaskBasedUpdater.Elevation;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.Operations;
using TaskBasedUpdater.Restart;

namespace TaskBasedUpdater
{
    public abstract class UpdateManager
    {
        protected readonly ILogger? Logger;
        private readonly List<IUpdateItem> _components = new List<IUpdateItem>();
        private readonly List<IUpdateItem> _removableComponents = new List<IUpdateItem>();
        private ReadOnlyCollection<IUpdateItem> _componentsReadOnly;
        private ReadOnlyCollection<IUpdateItem> _removableComponentsReadOnly;

        public Uri UpdateCatalogLocation { get; }

        public string ApplicationPath { get; }
        
        protected virtual IEnumerable<string> FileDeleteIgnoreFilter => new List<string>();

        protected virtual IEnumerable<string> FileDeleteExtensionFilter => new List<string> {".dll", ".exe"};

        public IReadOnlyCollection<IUpdateItem> AllComponents => Components.Union(RemovableComponents).ToList();

        public IReadOnlyCollection<IUpdateItem> Components => _componentsReadOnly ??= new ReadOnlyCollection<IUpdateItem>(_components);

        public IReadOnlyCollection<IUpdateItem> RemovableComponents => _removableComponentsReadOnly ??= new ReadOnlyCollection<IUpdateItem>(_removableComponents);

        protected UpdateManager(string applicationPath, string versionMetadataPath)
        {
            if (!Uri.TryCreate(versionMetadataPath, UriKind.Absolute, out var metadataUri))
                throw new UriFormatException();
            UpdateCatalogLocation = metadataUri;
            ApplicationPath = applicationPath;
        }

        public async Task<UpdateResult> UpdateAsync(CancellationToken cancellation)
        {
            var allComponents = Components.Concat(RemovableComponents);
            return await UpdateAsync(allComponents, cancellation);
        }

        public async Task CalculateComponentStatusAsync(CancellationToken cancellation = default)
        {
            Logger?.LogTrace("Calculating current component state");

            foreach (var component in Components)
            {
                cancellation.ThrowIfCancellationRequested();
                await Task.Yield();
                await CalculateComponentStatusAsync(component);
            }
        }

        public async Task<Stream> GetMetadataStreamAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            try
            {
                Stream metadataStream = new MemoryStream();
                await DownloadManager.Instance.DownloadAsync(UpdateCatalogLocation, metadataStream, null, cancellation);
                Logger.LogInformation($"Retrieved metadata stream from {UpdateCatalogLocation}");
                return metadataStream;
            }
            catch (OperationCanceledException)
            {
                Logger.LogTrace("Getting metadata stream was cancelled");
                throw;
            }
        }

        public async Task CalculateRemovableComponentsAsync()
        {
            await CalculateRemovableComponentsAsync(ApplicationPath);
        }

        public virtual async Task<UpdateInformation> CheckAndPerformUpdateAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            Logger?.LogInformation("Start automatic check and update...");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var updateInformation = new UpdateInformation();
            var finalCleanUp = true;

            try
            {
                try
                {
                    var stream = await GetMetadataStreamAsync(cts.Token);
                    cts.Token.ThrowIfCancellationRequested();

                    if (stream is null || stream.Length == 0)
                        throw new UpdaterException($"Unable to get the update metadata from: {UpdateCatalogLocation}");
                    if (!await ValidateCatalogStreamAsync(stream))
                        throw new UpdaterException("Stream validation for metadata failed. Download corrupted?");

                    try
                    {
                        var components = await GetCatalogComponentsAsync(stream, cts.Token);
                        if (components is null)
                        {
                            NoUpdateInformation(updateInformation);
                            return updateInformation;
                        }
                        _components.AddRange(components);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Failed processing catalog: {e.Message}");
                        throw;
                    }

                    await CalculateComponentStatusAsync(cts.Token);
                    await CalculateRemovableComponentsAsync();

                    cts.Token.ThrowIfCancellationRequested();

                    if (!RemovableComponents.Any() && (!Components.Any() || Components.All(x => x.RequiredAction == UpdateAction.Keep)))
                    {
                        NoUpdateInformation(updateInformation);
                        return updateInformation;
                    }

                    await UpdateAsync(cts.Token);

                    var pendingResult = await HandleLockedComponentsAsync(false, out var pc, cts.Token);
                    switch (pendingResult.Status)
                    {
                        case HandlePendingComponentsStatus.HandledButStillPending:
                        case HandlePendingComponentsStatus.Declined:
                            throw new RestartDeniedOrFailedException(pendingResult.Message);
                        case HandlePendingComponentsStatus.Restart:
                            finalCleanUp = false;
                            Restart(pc.ToList());
                            break;
                        default:
                            SuccessInformation(updateInformation, "Success");
                            break;
                    }
                }
                catch (Exception e)
                {
                    var throwFlag = false;
                    if (e.IsExceptionType<OperationCanceledException>())
                        CancelledInformation(updateInformation);
                    else if (e is AggregateException && e.IsExceptionType<UpdaterException>())
                        ErrorInformation(updateInformation, e.TryGetWrappedException()?.Message);
                    else if (e is UpdaterException)
                        ErrorInformation(updateInformation, e.Message);
                    else if (e.IsExceptionType<ElevationRequireException>())
                        throw;
                    else
                        throwFlag = true;

                    RestoreBackup();

                    if (throwFlag)
                        throw;
                }
            }
            catch (RestoreFailedException e)
            {
                OnRestoreFailed(e, updateInformation);
            }
            catch (ElevationRequireException e)
            {
                HandleElevationRequest(e, updateInformation);
            }
            finally
            {
                if (finalCleanUp)
                    await Clean();
            }

            return updateInformation;
        }

        internal Task CalculateRemovableComponentsAsync(string basePath)
        {
            if (basePath == null || !Directory.Exists(basePath))
                return Task.FromException(new DirectoryNotFoundException(nameof(basePath)));


            var localFiles = new DirectoryInfo(basePath).GetFilesByExtensions(FileDeleteExtensionFilter.ToArray());

            foreach (var file in localFiles)
            {
                if (FileDeleteIgnoreFilter.Any(x => file.Name.EndsWith(x)))
                    continue;

                if (!FileCanBeDeleted(file))
                    continue;

                Logger.LogInformation($"File marked to get deleted: {file.FullName}");

                var updateItem = new UpdateItem
                {
                    Name = file.Name,
                    DiskSize = file.Length,
                    CurrentState = CurrentState.Installed,
                    RequiredAction = UpdateAction.Delete,
                    Destination = file.DirectoryName
                };

                _removableComponents.Add(updateItem);
            }

            return Task.CompletedTask;
        }

        protected void RestoreBackup()
        {
            try
            {
                OnRestore();
            }
            catch (Exception restoreException)
            {
                var message =
                    $"Failed to restore from an unsuccessful update attempt: {restoreException.Message}. " +
                    "Please Restart your computer and try again!";
                Logger.LogError(message);
                throw new RestoreFailedException(message, restoreException);
            }
        }

        protected virtual void OnRestore()
        {
            BackupManager.Instance.RestoreAllBackups();
        }

        protected void NoUpdateInformation(UpdateInformation updateInformation, bool userNotification = false)
        {
            Logger?.LogDebug("No update was required");
            updateInformation.Result = UpdateResult.NoUpdate;
            updateInformation.Message = "No update was required";
            updateInformation.RequiresUserNotification = userNotification;
        }

        protected void SuccessInformation(UpdateInformation updateInformation, string message, bool requiresRestart = false, bool userNotification = true)
        {
            Logger?.LogDebug("Update was completed sucessfully");
            updateInformation.Result = requiresRestart ? UpdateResult.SuccessRestartRequired : UpdateResult.Success;
            updateInformation.Message = message;
            updateInformation.RequiresUserNotification = userNotification;
        }

        protected void ErrorInformation(UpdateInformation updateInformation, string errorMessage, bool userNotification = true)
        {
            Logger?.LogDebug($"Update failed with message: {errorMessage}");
            updateInformation.Result = UpdateResult.Failed;
            updateInformation.Message = errorMessage;
            updateInformation.RequiresUserNotification = userNotification;
        }

        protected void CancelledInformation(UpdateInformation updateInformation, bool userNotification = false)
        {
            Logger?.LogDebug("Operation was cancelled by user request");
            updateInformation.Result = UpdateResult.Cancelled;
            updateInformation.Message = "Operation cancelled by user request";
            updateInformation.RequiresUserNotification = userNotification;
        }

        protected abstract Task<IEnumerable<IUpdateItem>?> GetCatalogComponentsAsync(Stream catalogStream, CancellationToken token);

        protected abstract Task<bool> ValidateCatalogStreamAsync(Stream inputStream);

        protected abstract IRestartOptions CreateRestartOptions(IReadOnlyCollection<IUpdateItem>? components = null);
        
        protected virtual Task<PendingHandledResult> HandleLockedComponentsCoreAsync(ICollection<IUpdateItem> pendingComponents, ILockingProcessManager lockingProcessManager,
            bool ignoreSelfLocked, CancellationToken token)
        {
            return Task.FromResult(new PendingHandledResult(HandlePendingComponentsStatus.Declined, "Handling restart is not implemented"));
        }

        protected virtual Version? GetComponentVersion(IUpdateItem updateItem)
        {
            try
            {
                return UpdaterUtilities.GetAssemblyVersion(updateItem.GetFilePath());
            }
            catch
            {
                return null;
            }
        }

        protected virtual bool PermitElevationRequest()
        {
            return false;
        }

        protected virtual bool FileCanBeDeleted(FileInfo file)
        {
            return false;
        }

        protected virtual void OnRestoreFailed(Exception ex, UpdateInformation updateInformation)
        {
            ErrorInformation(updateInformation, ex.Message);
        }

        protected Task Clean()
        {
            try
            {
                new CleanOperation(null).Run();
            }
            catch (Exception e)
            {
                Logger?.LogTrace(e, $"Failed clean up: {e.Message}");
            }
            return Task.CompletedTask;
        }

        protected internal ICollection<IUpdateItem> GetPendingComponents(ICollection<string> files, out ILockingProcessManager lockingProcessManager)
        {
            var components = FindComponentsFromFiles(files).ToList();
            lockingProcessManager = LockingProcessManagerFactory.Create();
            lockingProcessManager.Register(files);
            return components;
        }

        protected internal void Restart(IReadOnlyList<IUpdateItem> components)
        {
            var options = CreateRestartOptions(components);
            ApplicationRestartManager.RestartApplication(options);
        }

        protected internal async Task<UpdateResult> UpdateAsync(IEnumerable<IUpdateItem> components,
            CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            Logger?.LogTrace("Performing update...");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var operation = new UpdateOperation(components, null);
            try
            {
                await Task.Run(() =>
                {
                    operation.Schedule();
                    operation.Run(cts.Token);
                }, cts.Token).ConfigureAwait(false);

                return UpdateResult.Success;
            }
            catch (OperationCanceledException e)
            {
                Logger?.LogError(e, $"Cancelled update: {e.Message}");
                throw;
            }
            catch (UpdateItemFailedException e)
            {
                Logger?.LogError(e, "Component Failed to update");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed update: {e.Message}");
                throw;
            }
        }
        
        protected internal Task CalculateComponentStatusAsync(IUpdateItem component)
        {
            Logger?.LogTrace($"Check dependency if update required: {component}");
            
            var destination = component.Destination;
            Logger?.LogTrace($"Dependency base path: {destination}");
            if (string.IsNullOrEmpty(destination))
                return Task.FromException(new InvalidOperationException());

            var filePath = component.GetFilePath();
            if (File.Exists(filePath))
            {
                var currentVersion = GetComponentVersion(component);
                if (currentVersion == null)
                {
                    Logger?.LogInformation($"Dependency marked to get updated: {component}");
                    component.CurrentState = CurrentState.None;
                    component.RequiredAction = UpdateAction.Update;
                    return Task.CompletedTask;
                }

                component.CurrentState = CurrentState.Installed;
                component.CurrentVersion = currentVersion;
                component.DiskSize = new FileInfo(filePath).Length;


                if (component.OriginInfo is null)
                    return Task.CompletedTask;

                var newVersion = component.OriginInfo.Version;

                if (newVersion != null && newVersion != currentVersion)
                {
                    Logger?.LogInformation($"Dependency marked to get updated: {component}");
                    component.RequiredAction = UpdateAction.Update;
                    return Task.CompletedTask;
                }

                if (component.OriginInfo.ValidationContext is null)
                {
                    Logger?.LogInformation($"Dependency marked to keep: {component}");
                    return Task.CompletedTask;
                }

                var hashResult = HashVerifier.VerifyFile(filePath, component.OriginInfo.ValidationContext);
                if (hashResult == ValidationResult.HashMismatch)
                {
                    Logger?.LogInformation($"Dependency marked to get updated: {component}");
                    component.RequiredAction = UpdateAction.Update;
                    return Task.CompletedTask;
                }

                Logger?.LogInformation($"Dependency marked to keep: {component}");
                return Task.CompletedTask;
            }

            Logger?.LogInformation($"Dependency marked to get updated: {component}");
            component.RequiredAction = UpdateAction.Update;
            return Task.CompletedTask;
        }
        
        private Task<PendingHandledResult> HandleLockedComponentsAsync(bool ignoreSelfLockedProcess, out IEnumerable<IUpdateItem> pendingComponents, CancellationToken token = default)
        {
            pendingComponents = Enumerable.Empty<IUpdateItem>();
            var lockedFiles = LockedFilesWatcher.Instance.LockedFiles.ToList();
            if (!lockedFiles.Any())
                return Task.FromResult(new PendingHandledResult(HandlePendingComponentsStatus.Handled));
            var allPendingComponents = GetPendingComponents(lockedFiles, out var p);
            var result = HandleLockedComponentsCoreAsync(allPendingComponents, p, ignoreSelfLockedProcess, token).Result;
            pendingComponents = GetPendingComponents(LockedFilesWatcher.Instance.LockedFiles, out _);
            return Task.FromResult(result);
        }

        private void HandleElevationRequest(ElevationRequireException e, UpdateInformation updateInformation)
        {
            var restoreBackup = true;
            try
            {
                if (Elevator.IsProcessElevated)
                    throw new UpdaterException("The process is already elevated", e);

                var lockedResult = HandleLockedComponentsAsync(true, out var pendingComponents).Result;
                switch (lockedResult.Status)
                {
                    case HandlePendingComponentsStatus.Declined:
                        ErrorInformation(updateInformation, lockedResult.Message);
                        return;
                }

                if (!PermitElevationRequest())
                {
                    ErrorInformation(updateInformation, "The update was stopped because the process needs to be elevated");
                    return;
                }

                try
                {
                    var allComponents = CreateRestartOptions(e.AggregateComponents().Union(pendingComponents).Distinct().ToList());
                    Elevator.RestartElevated(allComponents);
                    restoreBackup = false;
                }
                catch (Exception ex)
                {
                    if (!(ex is Win32Exception && ex.HResult == -2147467259))
                        throw;
                    // The elevation was not accepted by the user
                    CancelledInformation(updateInformation);
                }
            }
            finally
            {
                if (restoreBackup)
                    RestoreBackup();
            }
        }

        private IEnumerable<IUpdateItem> FindComponentsFromFiles(IEnumerable<string> files)
        {
            return files.Select(FindComponentsFromFile).Where(component => component != null);
        }

        private IUpdateItem? FindComponentsFromFile(string file)
        {
            return Components.Concat(RemovableComponents).FirstOrDefault(x => x.GetFilePath().Equals(file));
        }
    }
}
