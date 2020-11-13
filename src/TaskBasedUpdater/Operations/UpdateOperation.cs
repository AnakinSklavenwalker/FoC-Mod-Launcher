using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SimplePipeline;
using SimplePipeline.Runners;
using SimplePipeline.Tasks;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Download;
using TaskBasedUpdater.Elevation;
using TaskBasedUpdater.Restart;
using TaskBasedUpdater.Tasks;
using Validation;

namespace TaskBasedUpdater.Operations
{
    internal class UpdateOperation : IOperation
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;

        private readonly HashSet<IUpdateItem> _allComponents;
        private bool _scheduled;
        private bool? _planSuccessful;
        private readonly List<IUpdaterTask> _componentsToInstall = new List<IUpdaterTask>();
        private readonly List<IUpdaterTask> _componentsToRemove = new List<IUpdaterTask>();
        private readonly List<UpdateItemDownloadTask> _componentsToDownload = new List<UpdateItemDownloadTask>();
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
            _allComponents = new HashSet<IUpdateItem>(dependencies, UpdateItemIdentityComparer.Default);
        }

        public bool Plan()
        {
            Initialize();
            if (_allComponents.Count == 0)
            {
                var operationException = new InvalidOperationException("No packages were found to install/uninstall.");
                _logger.LogError(operationException, operationException.Message);
                _planSuccessful = false;
                return _planSuccessful.Value;
            }

            var downloadLookup = new Dictionary<IUpdateItem, UpdateItemDownloadTask>();
            foreach (var dependency in _allComponents)
            {
                var packageActivities = PlanInstallable(dependency, downloadLookup);
                if (dependency.RequiredAction == UpdateAction.Delete && packageActivities.Install != null)
                    _componentsToRemove.Add(packageActivities.Install);
            }

            _componentsToRemove.Reverse();
            _installsOrUninstalls = _componentsToRemove.Concat(_componentsToInstall);

            foreach (var installsOrUninstall in _installsOrUninstalls)
            {
                if (installsOrUninstall is UpdateItemInstallTask install && downloadLookup.ContainsKey(install.UpdateItem) && 
                    (install.Action != UpdateAction.Delete|| install.UpdateItem.RequiredAction != UpdateAction.Keep))
                    _componentsToDownload.Add(downloadLookup[install.UpdateItem]);
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
                
                var failedDownloads = _componentsToDownload.Where(p =>
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

            _componentsToDownload.ForEach(download => _downloads.Queue(download));
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

        private PackageActivities PlanInstallable(IUpdateItem component, Dictionary<IUpdateItem, UpdateItemDownloadTask> downloadLookup)
        {
            PackageActivities packageActivities = null;
            if (component != null)
            {
                var isPresent = component.CurrentState == CurrentState.Installed;

                if (component.RequiredAction == UpdateAction.Update || component.RequiredAction == UpdateAction.Keep)
                {
                    // TODO: Debug this and check if everything is correct!!!!
                    packageActivities = CreateDownloadInstallActivities(component, component.RequiredAction, isPresent);
                    if (packageActivities.Install != null)
                        _componentsToInstall.Add(packageActivities.Install);
                    if (packageActivities.Download != null)
                        downloadLookup[component] = packageActivities.Download;
                }

                if (component.RequiredAction == UpdateAction.Delete)
                {
                    packageActivities = CreateDownloadInstallActivities(component, component.RequiredAction, isPresent);
                    if (packageActivities.Download != null)
                        downloadLookup[component] = packageActivities.Download;
                }
            }
            return packageActivities;
        }

        private PackageActivities CreateDownloadInstallActivities(IUpdateItem component, UpdateAction action, bool isPresent)
        {
            UpdateItemDownloadTask downloadComponent;
            UpdateItemInstallTask install;

            if (DownloadRequired(action, component))
            {
                downloadComponent = new UpdateItemDownloadTask(null, component);
                downloadComponent.Canceled += (_, __) => _linkedCancellationTokenSource?.Cancel();
                install = new UpdateItemInstallTask(null, component, action, downloadComponent, isPresent);
            }
            else
            {
                downloadComponent = null;
                install = new UpdateItemInstallTask(null, component, action, isPresent);
            }
            
            return new PackageActivities
            {
                Download = downloadComponent,
                Install = install
            };
        }

        private bool DownloadRequired(UpdateAction action, IUpdateItem component)
        {
            if (action != UpdateAction.Update)
                return false;

            if (component.CurrentState == CurrentState.Downloaded && UpdateItemDownloadPathStorage.Instance.TryGetValue(component, out _))
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