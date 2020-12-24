using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SimplePipeline;
using SimplePipeline.Runners;
using SimplePipeline.Tasks;
using TaskBasedUpdater.Component;
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

        private readonly HashSet<ProductComponent> _allComponents;
        private bool _scheduled;
        private bool? _planSuccessful;
        private readonly List<IUpdaterTask> _itemsToInstall = new();
        private readonly List<IUpdaterTask> _itemToRemove = new();
        private readonly List<ComponentDownloadTask> _itemsToDownload = new();
        private readonly ICollection<ElevationRequestData> _elevationRequests = new HashSet<ElevationRequestData>();

        private IEnumerable<IPipelineTask> _installsOrUninstalls;

        private TaskRunner _installs;
        private AsyncTaskRunner _downloads;
        private AcquireMutexTask? _installMutexTask;

        private CancellationTokenSource? _linkedCancellationTokenSource;
        private Elevator _elevator;
        

        internal bool IsCancelled { get; private set; }

        internal bool RequiredProcessElevation { get; private set; }

        private static int ParallelDownload => 2;

        public UpdateOperation(IEnumerable<ProductComponent> dependencies, IServiceProvider serviceProvider)
        {
            Requires.NotNull(dependencies, nameof(dependencies));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
            _allComponents = new HashSet<ProductComponent>(dependencies, ProductComponentIdentityComparer.Default);
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

            var downloadLookup = new Dictionary<ProductComponent, ComponentDownloadTask>();
            foreach (var dependency in _allComponents)
            {
                var packageActivities = PlanInstallable(dependency, downloadLookup);
                if (dependency.RequiredAction == ComponentAction.Delete && packageActivities.Install != null)
                    _itemToRemove.Add(packageActivities.Install);
            }

            _itemToRemove.Reverse();
            _installsOrUninstalls = _itemToRemove.Concat(_itemsToInstall);

            foreach (var installsOrUninstall in _installsOrUninstalls)
            {
                if (installsOrUninstall is ComponentInstallTask install && downloadLookup.ContainsKey(install.ProductComponent) && 
                    (install.Action != ComponentAction.Delete|| install.ProductComponent.RequiredAction != ComponentAction.Keep))
                    _itemsToDownload.Add(downloadLookup[install.ProductComponent]);
            }

            _planSuccessful = true;
            return _planSuccessful.Value;
        }

        public void Run(CancellationToken token = default)
        {
            Schedule();
            var installsOrUninstalls = _installsOrUninstalls?.OfType<ComponentInstallTask>() ?? Enumerable.Empty<ComponentInstallTask>();

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
                    throw new ComponentFailedException(
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
            // TODO: split-projects
            //if (UpdateConfiguration.Instance.RequiredElevationCancelsUpdate)
            //    _linkedCancellationTokenSource?.Cancel();
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

        private PackageActivities PlanInstallable(ProductComponent productComponent, Dictionary<ProductComponent, ComponentDownloadTask> downloadLookup)
        {
            PackageActivities packageActivities = null;
            if (productComponent != null)
            {
                var isPresent = productComponent.CurrentState == CurrentState.Installed;

                if (productComponent.RequiredAction == ComponentAction.Update || 
                    productComponent.RequiredAction == ComponentAction.Keep)
                {
                    // TODO: Debug this and check if everything is correct!!!!
                    packageActivities = CreateDownloadInstallActivities(productComponent, productComponent.RequiredAction, isPresent);
                    if (packageActivities.Install != null)
                        _itemsToInstall.Add(packageActivities.Install);
                    if (packageActivities.Download != null)
                        downloadLookup[productComponent] = packageActivities.Download;
                }

                if (productComponent.RequiredAction == ComponentAction.Delete)
                {
                    packageActivities = CreateDownloadInstallActivities(productComponent, productComponent.RequiredAction, isPresent);
                    if (packageActivities.Download != null)
                        downloadLookup[productComponent] = packageActivities.Download;
                }
            }
            return packageActivities;
        }

        private PackageActivities CreateDownloadInstallActivities(ProductComponent productComponent, ComponentAction action, bool isPresent)
        {
            ComponentDownloadTask downloadTask;
            ComponentInstallTask install;

            if (DownloadRequired(action, productComponent))
            {
                downloadTask = new ComponentDownloadTask(null, productComponent);
                downloadTask.Canceled += (_, __) => _linkedCancellationTokenSource?.Cancel();
                install = new ComponentInstallTask(null, productComponent, action, downloadTask, isPresent);
            }
            else
            {
                downloadTask = null;
                install = new ComponentInstallTask(null, productComponent, action, isPresent);
            }
            
            return new PackageActivities
            {
                Download = downloadTask,
                Install = install
            };
        }

        private bool DownloadRequired(ComponentAction action, ProductComponent productComponent)
        {
            if (action != ComponentAction.Update)
                return false;

            if (productComponent.CurrentState == CurrentState.Downloaded && UpdateItemDownloadPathStorage.Instance.TryGetValue(productComponent, out _))
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
                if (e.Cancel || !(e.Task is IUpdaterTask))
                    return;
                
                if (e.Task is ComponentInstallTask installTask)
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
                else if (e.Task is ComponentDownloadTask)
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
            internal ComponentDownloadTask Download { get; init; }

            internal ComponentInstallTask Install { get; init; }
        }
    }
}