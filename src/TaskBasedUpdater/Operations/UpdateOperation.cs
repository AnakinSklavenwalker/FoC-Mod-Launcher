using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SimplePipeline;
using SimplePipeline.Runners;
using SimplePipeline.Tasks;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Download;
using TaskBasedUpdater.Elevation;
using TaskBasedUpdater.Restart;
using TaskBasedUpdater.Tasks;
using TaskBasedUpdater.UpdateItem;
using Validation;

namespace TaskBasedUpdater.Operations
{
    internal class UpdateOperation : IOperation
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;

        private readonly HashSet<IUpdateItem> _allUpdateItems;
        private bool _scheduled;
        private bool? _planSuccessful;
        private readonly List<IUpdaterTask> _itemsToInstall = new List<IUpdaterTask>();
        private readonly List<IUpdaterTask> _itemToRemove = new List<IUpdaterTask>();
        private readonly List<UpdateItemDownloadTask> _itemsToDownload = new List<UpdateItemDownloadTask>();
        private readonly ICollection<ElevationRequestData> _elevationRequests = new HashSet<ElevationRequestData>();

        private IEnumerable<IPipelineTask> _installsOrUninstalls;

        private TaskRunner _installs;
        private AsyncTaskRunner _downloads;
        private IDownloadManager _downloadManager;
        private AcquireMutexTask? _installMutexTask;

        private CancellationTokenSource? _linkedCancellationTokenSource;
        private Elevator _elevator;
        

        internal bool IsCancelled { get; private set; }

        internal bool RequiredProcessElevation { get; private set; }

        private static int ParallelDownload => 2;

        public UpdateOperation(IEnumerable<IUpdateItem> dependencies, IServiceProvider serviceProvider)
        {
            Requires.NotNull(dependencies, nameof(dependencies));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
            _allUpdateItems = new HashSet<IUpdateItem>(dependencies, UpdateItemIdentityComparer.Default);
        }

        public bool Plan()
        {
            Initialize();
            if (_allUpdateItems.Count == 0)
            {
                var operationException = new InvalidOperationException("No packages were found to install/uninstall.");
                _logger.LogError(operationException, operationException.Message);
                _planSuccessful = false;
                return _planSuccessful.Value;
            }

            var downloadLookup = new Dictionary<IUpdateItem, UpdateItemDownloadTask>();
            foreach (var dependency in _allUpdateItems)
            {
                var packageActivities = PlanInstallable(dependency, downloadLookup);
                if (dependency.RequiredAction == UpdateAction.Delete && packageActivities.Install != null)
                    _itemToRemove.Add(packageActivities.Install);
            }

            _itemToRemove.Reverse();
            _installsOrUninstalls = _itemToRemove.Concat(_itemsToInstall);

            foreach (var installsOrUninstall in _installsOrUninstalls)
            {
                if (installsOrUninstall is UpdateItemInstallTask install && downloadLookup.ContainsKey(install.UpdateItem) && 
                    (install.Action != UpdateAction.Delete|| install.UpdateItem.RequiredAction != UpdateAction.Keep))
                    _itemsToDownload.Add(downloadLookup[install.UpdateItem]);
            }

            _planSuccessful = true;
            return _planSuccessful.Value;
        }

        public void Run(CancellationToken token = default)
        {
            Schedule();
            var installsOrUninstalls = _installsOrUninstalls?.OfType<UpdateItemInstallTask>() ?? Enumerable.Empty<UpdateItemInstallTask>();

            using var mutex = UpdaterUtilities.CheckAndSetGlobalMutex();
            try
            {
                try
                {
                    _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    _downloads.Run(_linkedCancellationTokenSource.Token);
                    _installs.Run(_linkedCancellationTokenSource.Token);

                    try
                    {
                        _downloads.Wait();
                    }
                    catch (Exception)
                    {
                    }
                }
                finally
                {
                    if (_linkedCancellationTokenSource != null)
                    {
                        _linkedCancellationTokenSource.Dispose();
                        _linkedCancellationTokenSource = null;
                    }

                    _logger.LogTrace("Completed update operation");
                }

                if (RequiredProcessElevation)
                    throw new ElevationRequireException(_elevationRequests);

                if (IsCancelled)
                    throw new OperationCanceledException(token);
                token.ThrowIfCancellationRequested();
                
                var failedDownloads = _itemsToDownload.Where(p =>
                    p.Error != null && !p.Error.IsExceptionType<OperationCanceledException>());

                var failedInstalls = installsOrUninstalls
                    .Where(installTask => !installTask.Result.IsSuccess()).ToList();

                if (failedDownloads.Any() || failedInstalls.Any())
                    throw new UpdateItemFailedException(
                        "Update failed because one or more downloads or installs had an error.");

                var requiresRestart = LockedFilesWatcher.Instance.LockedFiles.Any();
                if (requiresRestart)
                    _logger.LogInformation("The operation finished. A restart is pedning.");
            }
            finally
            {
                mutex.ReleaseMutex();
                _installMutexTask?.Dispose();
            }
        }

        private void OnElevationRequested(object sender, ElevationRequestData e)
        {
            _logger.LogInformation($"Elevation requested: {e.Exception.Message}");
            RequiredProcessElevation = true;
            _elevationRequests.Add(e);
            if (UpdateConfiguration.Instance.RequiredElevationCancelsUpdate)
                _linkedCancellationTokenSource?.Cancel();
        }

        internal void Schedule()
        {
            if (_scheduled)
                return;
            Initialize();
            if (!Plan())
                return;

            _itemsToDownload.ForEach(download => _downloads.Queue(download));
            QueueInitialActivities();
            foreach (var installsOrUninstall in _installsOrUninstalls)
                _installs.Queue(installsOrUninstall);
            _scheduled = true;
        }

        private void Initialize()
        {
            if (_downloadManager == null)
                _downloadManager = DownloadManager.Instance;

            if (_elevator == null)
            {
                _elevator = Elevator.Instance;
                _elevator.ElevationRequested += OnElevationRequested;
            }
            if (_downloads == null)
            {
                var workers = ParallelDownload;
                _logger?.LogTrace($"Concurrent downloads: {workers}");
                _downloads = new AsyncTaskRunner(null, workers);
                _downloads.Error += OnError;
            }
            if (_installs == null)
            {
                _installs = new TaskRunner(null);
                _installs.Error += OnError;
            }
        }

        private void CleanEvents()
        {
            if (_downloads != null)
                _downloads.Error -= OnError;
            if (_installs != null)
                _installs.Error -= OnError;
            _elevator.ElevationRequested -= OnElevationRequested;
        }
        
        private void QueueInitialActivities()
        {
            //_installs.Queue(new WaitTask(_downloads)); // Waits until all downloads are finished
            _installMutexTask = new AcquireMutexTask(null);
            _installs.Queue(_installMutexTask);
        }

        private PackageActivities PlanInstallable(IUpdateItem updateItem, Dictionary<IUpdateItem, UpdateItemDownloadTask> downloadLookup)
        {
            PackageActivities packageActivities = null;
            if (updateItem != null)
            {
                var isPresent = updateItem.CurrentState == CurrentState.Installed;

                if (updateItem.RequiredAction == UpdateAction.Update || updateItem.RequiredAction == UpdateAction.Keep)
                {
                    // TODO: Debug this and check if everything is correct!!!!
                    packageActivities = CreateDownloadInstallActivities(updateItem, updateItem.RequiredAction, isPresent);
                    if (packageActivities.Install != null)
                        _itemsToInstall.Add(packageActivities.Install);
                    if (packageActivities.Download != null)
                        downloadLookup[updateItem] = packageActivities.Download;
                }

                if (updateItem.RequiredAction == UpdateAction.Delete)
                {
                    packageActivities = CreateDownloadInstallActivities(updateItem, updateItem.RequiredAction, isPresent);
                    if (packageActivities.Download != null)
                        downloadLookup[updateItem] = packageActivities.Download;
                }
            }
            return packageActivities;
        }

        private PackageActivities CreateDownloadInstallActivities(IUpdateItem updateItem, UpdateAction action, bool isPresent)
        {
            UpdateItemDownloadTask downloadTask;
            UpdateItemInstallTask install;

            if (DownloadRequired(action, updateItem))
            {
                downloadTask = new UpdateItemDownloadTask(null, updateItem);
                downloadTask.Canceled += (_, __) => _linkedCancellationTokenSource?.Cancel();
                install = new UpdateItemInstallTask(null, updateItem, action, downloadTask, isPresent);
            }
            else
            {
                downloadTask = null;
                install = new UpdateItemInstallTask(null, updateItem, action, isPresent);
            }
            
            return new PackageActivities
            {
                Download = downloadTask,
                Install = install
            };
        }

        private bool DownloadRequired(UpdateAction action, IUpdateItem updateItem)
        {
            if (action != UpdateAction.Update)
                return false;

            if (updateItem.CurrentState == CurrentState.Downloaded && UpdateItemDownloadPathStorage.Instance.TryGetValue(updateItem, out _))
                return false;


            return true;
        }

        private void OnError(object sender, TaskEventArgs e)
        {
            IsCancelled |= e.Cancel;
            if (e.Cancel)
                _linkedCancellationTokenSource?.Cancel();
            try
            {
                if (e.Cancel || !(e.Task is IUpdaterTask updaterTask))
                    return;
                
                if (e.Task is UpdateItemInstallTask installTask)
                {
                    if (installTask.Result.IsFailure())
                    {
                        // TODO
                    }
                    else
                    {
                        // TODO
                    }
                }
                else if (e.Task is UpdateItemDownloadTask downloadTask)
                {
                    // TODO
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Skipping as an error occurred");
            }
        }

        private class PackageActivities
        {
            internal UpdateItemDownloadTask Download { get; set; }

            internal UpdateItemInstallTask Install { get; set; }
        }
    }
}