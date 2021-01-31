namespace ProductUpdater
{
    /*
    public abstract class UpdateManager
    {
        protected readonly ILogger? Logger;
        private readonly List<IUpdateItem> _updateItems = new List<IUpdateItem>();
        private readonly List<IUpdateItem> _removableUpdateItems = new List<IUpdateItem>();
        private ReadOnlyCollection<IUpdateItem> _itemsReadOnly;
        private ReadOnlyCollection<IUpdateItem> _removableItemsReadOnly;

        public Uri UpdateCatalogLocation { get; }

        public string ApplicationPath { get; }
        
        protected virtual IEnumerable<string> FileDeleteIgnoreFilter => new List<string>();

        protected virtual IEnumerable<string> FileDeleteExtensionFilter => new List<string> {".dll", ".exe"};

        public IReadOnlyCollection<IUpdateItem> AllUpdateItems => UpdateItems.Union(RemovableUpdateItems).ToList();

        public IReadOnlyCollection<IUpdateItem> UpdateItems => _itemsReadOnly ??= new ReadOnlyCollection<IUpdateItem>(_updateItems);

        public IReadOnlyCollection<IUpdateItem> RemovableUpdateItems => _removableItemsReadOnly ??= new ReadOnlyCollection<IUpdateItem>(_removableUpdateItems);

        protected UpdateManager(string applicationPath, string versionMetadataPath)
        {
            if (!Uri.TryCreate(versionMetadataPath, UriKind.Absolute, out var metadataUri))
                throw new UriFormatException();
            UpdateCatalogLocation = metadataUri;
            ApplicationPath = applicationPath;
        }

        public async Task<UpdateResult> UpdateAsync(CancellationToken cancellation)
        {
            var allItems = UpdateItems.Concat(RemovableUpdateItems);
            return await UpdateAsync(allItems, cancellation);
        }

        public async Task CalculateUpdateItemsStatusAsync(CancellationToken cancellation = default)
        {
            Logger?.LogTrace("Calculating current item state");

            foreach (var item in UpdateItems)
            {
                cancellation.ThrowIfCancellationRequested();
                await Task.Yield();
                await CalculateUpdateItemsStatusAsync(item);
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

        public async Task CalculateRemovableUpdateItemsAsync()
        {
            await CalculateRemovableUpdateItemsAsync(ApplicationPath);
        }

        public virtual async Task<UpdateResultInformation> CheckAndPerformUpdateAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            Logger?.LogInformation("Start automatic check and update...");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var updateInformation = new UpdateResultInformation();
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
                        var items = await GetCatalogItemsAsync(stream, cts.Token);
                        if (items is null)
                        {
                            NoUpdateInformation(updateInformation);
                            return updateInformation;
                        }
                        _updateItems.AddRange(items);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Failed processing catalog: {e.Message}");
                        throw;
                    }

                    await CalculateUpdateItemsStatusAsync(cts.Token);
                    await CalculateRemovableUpdateItemsAsync();

                    cts.Token.ThrowIfCancellationRequested();

                    if (!RemovableUpdateItems.Any() && (!UpdateItems.Any() || UpdateItems.All(x => x.RequiredAction == UpdateAction.Keep)))
                    {
                        NoUpdateInformation(updateInformation);
                        return updateInformation;
                    }

                    await UpdateAsync(cts.Token);

                    var pendingResult = await HandleLockedItemsAsync(false, out var pc, cts.Token);
                    switch (pendingResult.Status)
                    {
                        case HandlePendingItemStatus.HandledButStillPending:
                        case HandlePendingItemStatus.Declined:
                            throw new RestartDeniedOrFailedException(pendingResult.Message);
                        case HandlePendingItemStatus.Restart:
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

        internal Task CalculateRemovableUpdateItemsAsync(string basePath)
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

                _removableUpdateItems.Add(updateItem);
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

        protected void NoUpdateInformation(UpdateResultInformation updateResultInformation, bool userNotification = false)
        {
            Logger?.LogDebug("No update was required");
            updateResultInformation.Result = UpdateResult.NoUpdate;
            updateResultInformation.Message = "No update was required";
            updateResultInformation.RequiresUserNotification = userNotification;
        }

        protected void SuccessInformation(UpdateResultInformation updateResultInformation, string message, bool requiresRestart = false, bool userNotification = true)
        {
            Logger?.LogDebug("Update was completed sucessfully");
            updateResultInformation.Result = requiresRestart ? UpdateResult.SuccessRestartRequired : UpdateResult.Success;
            updateResultInformation.Message = message;
            updateResultInformation.RequiresUserNotification = userNotification;
        }

        protected void ErrorInformation(UpdateResultInformation updateResultInformation, string errorMessage, bool userNotification = true)
        {
            Logger?.LogDebug($"Update failed with message: {errorMessage}");
            updateResultInformation.Result = UpdateResult.Failed;
            updateResultInformation.Message = errorMessage;
            updateResultInformation.RequiresUserNotification = userNotification;
        }

        protected void CancelledInformation(UpdateResultInformation updateResultInformation, bool userNotification = false)
        {
            Logger?.LogDebug("Operation was cancelled by user request");
            updateResultInformation.Result = UpdateResult.Cancelled;
            updateResultInformation.Message = "Operation cancelled by user request";
            updateResultInformation.RequiresUserNotification = userNotification;
        }

        protected abstract Task<IEnumerable<IUpdateItem>?> GetCatalogItemsAsync(Stream catalogStream, CancellationToken token);

        protected abstract Task<bool> ValidateCatalogStreamAsync(Stream inputStream);

        protected abstract IRestartOptions CreateRestartOptions(IReadOnlyCollection<IUpdateItem>? updateItems = null);
        
        protected virtual Task<PendingHandledResult> HandleLockedItemsCoreAsync(ICollection<IUpdateItem> pendingItems, ILockingProcessManager lockingProcessManager,
            bool ignoreSelfLocked, CancellationToken token)
        {
            return Task.FromResult(new PendingHandledResult(HandlePendingItemStatus.Declined, "Handling restart is not implemented"));
        }

        protected virtual Version? GetFileVersion(IUpdateItem updateItem)
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

        protected virtual void OnRestoreFailed(Exception ex, UpdateResultInformation updateResultInformation)
        {
            ErrorInformation(updateResultInformation, ex.Message);
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

        protected internal ICollection<IUpdateItem> GetPendingItems(ICollection<string> files, out ILockingProcessManager lockingProcessManager)
        {
            var items = FindUpdateItemsFromFiles(files).ToList();
            lockingProcessManager = LockingProcessManagerFactory.Create();
            lockingProcessManager.Register(files);
            return items;
        }

        protected internal void Restart(IReadOnlyList<IUpdateItem> items)
        {
            var options = CreateRestartOptions(items);
            ApplicationRestartManager.RestartApplication(options);
        }

        protected internal async Task<UpdateResult> UpdateAsync(IEnumerable<IUpdateItem> items,
            CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            Logger?.LogTrace("Performing update...");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var operation = new UpdateOperation(items, null);
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
                Logger?.LogError(e, "Update Item Failed to update");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed update: {e.Message}");
                throw;
            }
        }
        
        protected internal Task CalculateUpdateItemsStatusAsync(IUpdateItem item)
        {
            Logger?.LogTrace($"Check dependency if update required: {item}");
            
            var destination = item.Destination;
            Logger?.LogTrace($"Dependency base path: {destination}");
            if (string.IsNullOrEmpty(destination))
                return Task.FromException(new InvalidOperationException());

            var filePath = item.GetFilePath();
            if (File.Exists(filePath))
            {
                var currentVersion = GetFileVersion(item);
                if (currentVersion == null)
                {
                    Logger?.LogInformation($"Dependency marked to get updated: {item}");
                    item.CurrentState = CurrentState.None;
                    item.RequiredAction = UpdateAction.Update;
                    return Task.CompletedTask;
                }

                item.CurrentState = CurrentState.Installed;
                item.CurrentVersion = currentVersion;
                item.DiskSize = new FileInfo(filePath).Length;


                if (item.OriginInfo is null)
                    return Task.CompletedTask;

                var newVersion = item.OriginInfo.Version;

                if (newVersion != null && newVersion != currentVersion)
                {
                    Logger?.LogInformation($"Dependency marked to get updated: {item}");
                    item.RequiredAction = UpdateAction.Update;
                    return Task.CompletedTask;
                }

                if (item.OriginInfo.VerificationContext is null)
                {
                    Logger?.LogInformation($"Dependency marked to keep: {item}");
                    return Task.CompletedTask;
                }

                var hashResult = HashVerifier.VerifyFile(filePath, item.OriginInfo.VerificationContext);
                if (hashResult == ValidationResult.HashMismatch)
                {
                    Logger?.LogInformation($"Dependency marked to get updated: {item}");
                    item.RequiredAction = UpdateAction.Update;
                    return Task.CompletedTask;
                }

                Logger?.LogInformation($"Dependency marked to keep: {item}");
                return Task.CompletedTask;
            }

            Logger?.LogInformation($"Dependency marked to get updated: {item}");
            item.RequiredAction = UpdateAction.Update;
            return Task.CompletedTask;
        }
        
        private Task<PendingHandledResult> HandleLockedItemsAsync(bool ignoreSelfLockedProcess, out IEnumerable<IUpdateItem> pendingItems, CancellationToken token = default)
        {
            pendingItems = Enumerable.Empty<IUpdateItem>();
            var lockedFiles = LockedFilesWatcher.Instance.LockedFiles.ToList();
            if (!lockedFiles.Any())
                return Task.FromResult(new PendingHandledResult(HandlePendingItemStatus.Handled));
            var allPendingItems = GetPendingItems(lockedFiles, out var p);
            var result = HandleLockedItemsCoreAsync(allPendingItems, p, ignoreSelfLockedProcess, token).Result;
            pendingItems = GetPendingItems(LockedFilesWatcher.Instance.LockedFiles, out _);
            return Task.FromResult(result);
        }

        private void HandleElevationRequest(ElevationRequireException e, UpdateResultInformation updateResultInformation)
        {
            var restoreBackup = true;
            try
            {
                if (Elevator.IsProcessElevated)
                    throw new UpdaterException("The process is already elevated", e);

                var lockedResult = HandleLockedItemsAsync(true, out var pendingItems).Result;
                switch (lockedResult.Status)
                {
                    case HandlePendingItemStatus.Declined:
                        ErrorInformation(updateResultInformation, lockedResult.Message);
                        return;
                }

                if (!PermitElevationRequest())
                {
                    ErrorInformation(updateResultInformation, "The update was stopped because the process needs to be elevated");
                    return;
                }

                try
                {
                    var restartOptions = CreateRestartOptions(e.AggregateItems().Union(pendingItems).Distinct().ToList());
                    Elevator.RestartElevated(restartOptions);
                    restoreBackup = false;
                }
                catch (Exception ex)
                {
                    if (!(ex is Win32Exception && ex.HResult == -2147467259))
                        throw;
                    // The elevation was not accepted by the user
                    CancelledInformation(updateResultInformation);
                }
            }
            finally
            {
                if (restoreBackup)
                    RestoreBackup();
            }
        }

        private IEnumerable<IUpdateItem> FindUpdateItemsFromFiles(IEnumerable<string> files)
        {
            return files.Select(FindUpdateItemFromFile).Where(item => item != null);
        }

        private IUpdateItem? FindUpdateItemFromFile(string file)
        {
            return UpdateItems.Concat(RemovableUpdateItems).FirstOrDefault(x => x.GetFilePath().Equals(file));
        }
    }
    */
}
