using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimplePipeline;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Download;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Product.Component;
using TaskBasedUpdater.New.Update;
using TaskBasedUpdater.Tasks;
using Validation;

namespace TaskBasedUpdater.Operations
{
    internal class UpdateOperation : IOperation
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<ProductComponent> _components;
        private readonly IInstalledProduct _product;
        
        private readonly IList<IComponentTask> _componentsToInstall = new List<IComponentTask>();
        private readonly IList<ComponentDownloadTask> _componentsToDownload = new List<ComponentDownloadTask>();
        private readonly IList<IComponentTask> _componentsToDelete = new List<IComponentTask>();
        private readonly IList<IComponentTask> _componentsToVerify = new List<IComponentTask>();

        private bool? _planSuccessful;
        private bool _scheduled;
        private AsyncPipelineRunner? _downloadsRunner;
        private PipelineRunner? _installsRunner;
        private PipelineRunner? _cancelCleanupRunner;
        private IDownloadManager? _downloadManager;
        private ILogger? _logger;

        public IReadOnlyCollection<IComponentTask> ComponentsToInstall => new ReadOnlyCollection<IComponentTask>(_componentsToInstall);
        public IReadOnlyCollection<ComponentDownloadTask> ComponentsToDownload => new ReadOnlyCollection<ComponentDownloadTask>(_componentsToDownload);
        public IReadOnlyCollection<IComponentTask> ComponentsToDelete => new ReadOnlyCollection<IComponentTask>(_componentsToDelete);
        public IReadOnlyCollection<IComponentTask> ComponentsToVerify => new ReadOnlyCollection<IComponentTask>(_componentsToVerify);

        public long DownloadSize => _componentsToDownload.Sum(download => download.ProductComponent.OriginInfo?.Size ?? 0);
        
        public UpdateConfiguration UpdateConfiguration { get; }

        internal IEnumerable<IPipelineTask> DownloadActivities => _downloadsRunner ?? Enumerable.Empty<IPipelineTask>();

        internal IEnumerable<IPipelineTask> InstallActivities => _installsRunner ?? Enumerable.Empty<IPipelineTask>();

        internal IEnumerable<IPipelineTask> CancelCleanupActivities => _cancelCleanupRunner ?? Enumerable.Empty<IPipelineTask>();

        private bool IsCancelled { get; set; }

        public UpdateOperation(IInstalledProduct product, UpdateConfiguration updateConfiguration, IEnumerable<ProductComponent> components, IServiceProvider serviceProvider)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(components, nameof(components));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            _product = product;
            _serviceProvider = serviceProvider;
            _components = new HashSet<ProductComponent>(components, ProductComponentIdentityComparer.Default);
            UpdateConfiguration = updateConfiguration;
        }


        public bool Plan()
        {
            if (_planSuccessful.HasValue)
                return _planSuccessful.Value;
            Initialize();
            if (!_components.Any())
            {
                InvalidOperationException operationException = new("No packages were found to install/uninstall.");
                _logger.LogError(operationException, operationException.Message);
                _planSuccessful = false;
                return _planSuccessful.Value;
            }
            var downloadLookup = new Dictionary<ProductComponent, ComponentDownloadTask>(ProductComponentIdentityComparer.Default);
            foreach (var component in _components)
            {
                var plan = new ComponentPlan(component);
                var activities = PlanComponent(component, downloadLookup, ref plan);
            }
            return false;
        }

        private PackageActivities PlanComponent(
            ProductComponent component, 
            IDictionary<ProductComponent, ComponentDownloadTask> downloadLookup, 
            ref ComponentPlan plan)
        {
            var action = component.RequiredAction;
            var isPresent = component.DetectedState == DetectionState.Present;
            var plannedAction = ComponentAction.Undecided;
            return null;
        }

        public void Run(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        internal void Schedule()
        {
            if (_scheduled)
                return;
            Initialize();
            if (!Plan())
                return;
            if (UpdateConfiguration.InstallMode == InstallMode.DownloadOnly)
            {
            }
            else
            {
                
            }
            _scheduled = true;
        }

        private void Initialize()
        {
            _downloadManager ??= _serviceProvider.GetRequiredService<IDownloadManager>();
            _logger ??= _serviceProvider.GetService<ILogger>();

            if (_downloadsRunner is null)
            {
                var threads = UpdateConfiguration.ConcurrentDownloads;
                if (threads < 1 || threads > 64)
                {
                    _logger?.LogWarning($"Overriding the ConcurrentDownloads value: {threads}");
                    threads = 2;
                }
                _logger?.LogTrace($"Concurrent downloads: {threads}");
                _downloadsRunner = new AsyncPipelineRunner(_serviceProvider, threads);
                _downloadsRunner.Error += OnError;
            }
            if (_installsRunner == null)
            {
                _installsRunner = new PipelineRunner(_serviceProvider);
                _installsRunner.Error += OnError;
            }
            if (_cancelCleanupRunner == null)
            {
                _cancelCleanupRunner = new PipelineRunner(_serviceProvider);
                _cancelCleanupRunner.Error += OnError;
            }

        }

        private void OnError(object sender, TaskEventArgs e)
        {
            throw new NotImplementedException();
        }

        private record PackageActivities
        {
            internal ComponentDownloadTask? Download { get; init; }

            internal ComponentInstallTask? Install { get; init; }
        }
    }

    internal class ComponentFileEvaluator : IComponentEvaluator
    {
        public bool Evaluate(ProductComponent productComponent)
        {
            throw new NotImplementedException();
        }
    }

    public interface IComponentEvaluator
    {
        bool Evaluate(ProductComponent productComponent);
    }


    internal class ComponentPlan
    {
        private readonly ProductComponent _component;

        public ComponentAction RequiredAction { get; }
        public DetectionState DetectionState { get; }

        public ComponentPlan(ProductComponent component)
        {
            Requires.NotNull(component, nameof(component));
            RequiredAction = component.RequiredAction;
            DetectionState = component.DetectedState;
            _component = component;
        }
    }



    class OldOperation
    {
        //public bool Plan()
        //{
        //    Initialize();
        //    if (_allComponents.Count == 0)
        //    {
        //        var operationException = new InvalidOperationException("No packages were found to install/uninstall.");
        //        _logger.LogError(operationException, operationException.Message);
        //        _planSuccessful = false;
        //        return _planSuccessful.Value;
        //    }

        //    var downloadLookup = new Dictionary<ProductComponent, ComponentDownloadTask>();
        //    foreach (var dependency in _allComponents)
        //    {
        //        var packageActivities = PlanInstallable(dependency, downloadLookup);
        //        if (dependency.RequiredAction == ComponentAction.Delete && packageActivities.Install != null)
        //            _itemToRemove.Add(packageActivities.Install);
        //    }

        //    _itemToRemove.Reverse();
        //    _installsOrUninstalls = _itemToRemove.Concat(_itemsToInstall);

        //    foreach (var installsOrUninstall in _installsOrUninstalls)
        //    {
        //        if (installsOrUninstall is ComponentInstallTask install && downloadLookup.ContainsKey(install.ProductComponent) && 
        //            (install.Action != ComponentAction.Delete|| install.ProductComponent.RequiredAction != ComponentAction.Keep))
        //            _itemsToDownload.Add(downloadLookup[install.ProductComponent]);
        //    }

        //    _planSuccessful = true;
        //    return _planSuccessful.Value;
        //}

        //public void Run(CancellationToken token = default)
        //{
        //    Schedule();
        //    var installsOrUninstalls = _installsOrUninstalls?.OfType<ComponentInstallTask>() ?? Enumerable.Empty<ComponentInstallTask>();

        //    using var mutex = UpdaterUtilities.CheckAndSetGlobalMutex();
        //    try
        //    {
        //        try
        //        {
        //            _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        //            _downloads.Run(_linkedCancellationTokenSource.Token);
        //            _installs.Run(_linkedCancellationTokenSource.Token);

        //            try
        //            {
        //                _downloads.Wait();
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }
        //        finally
        //        {
        //            if (_linkedCancellationTokenSource != null)
        //            {
        //                _linkedCancellationTokenSource.Dispose();
        //                _linkedCancellationTokenSource = null;
        //            }

        //            _logger.LogTrace("Completed update operation");
        //        }

        //        if (RequiredProcessElevation)
        //            throw new ElevationRequireException(_elevationRequests);

        //        if (IsCancelled)
        //            throw new OperationCanceledException(token);
        //        token.ThrowIfCancellationRequested();

        //        var failedDownloads = _itemsToDownload.Where(p =>
        //            p.Error != null && !p.Error.IsExceptionType<OperationCanceledException>());

        //        var failedInstalls = installsOrUninstalls
        //            .Where(installTask => !installTask.Result.IsSuccess()).ToList();

        //        if (failedDownloads.Any() || failedInstalls.Any())
        //            throw new ComponentFailedException(
        //                "Update failed because one or more downloads or installs had an error.");

        //        var requiresRestart = LockedFilesWatcher.Instance.LockedFiles.Any();
        //        if (requiresRestart)
        //            _logger.LogInformation("The operation finished. A restart is pedning.");
        //    }
        //    finally
        //    {
        //        mutex.ReleaseMutex();
        //        _installMutexTask?.Dispose();
        //    }
        //}

        //private void OnElevationRequested(object sender, ElevationRequestData e)
        //{
        //    _logger.LogInformation($"Elevation requested: {e.Exception.Message}");
        //    RequiredProcessElevation = true;
        //    _elevationRequests.Add(e);
        //    if (Configuration.RequiredElevationCancelsUpdate)
        //        _linkedCancellationTokenSource?.Cancel();
        //}

        //internal void Schedule()
        //{
        //    if (_scheduled)
        //        return;
        //    Initialize();
        //    if (!Plan())
        //        return;

        //    _itemsToDownload.ForEach(download => _downloads.Queue(download));
        //    QueueInitialActivities();
        //    foreach (var installsOrUninstall in _installsOrUninstalls)
        //        _installs.Queue(installsOrUninstall);
        //    _scheduled = true;
        //}

        //private void Initialize()
        //{
        //    if (_elevator is null)
        //    {
        //        _elevator = Elevator.Instance;
        //        _elevator.ElevationRequested += OnElevationRequested;
        //    }

        //    if (_logger is null)
        //        _logger = _serviceProvider.GetService<ILogger>();
        //    if (_downloads is null)
        //    {
        //        var workers = ParallelDownload;
        //        _logger?.LogTrace($"Concurrent downloads: {workers}");
        //        _downloads = new AsyncPipelineRunner(_serviceProvider, workers);
        //        _downloads.Error += OnError;
        //    }
        //    if (_installs is null)
        //    {
        //        _installs = new TaskRunner(_serviceProvider);
        //        _installs.Error += OnError;
        //    }
        //}

        //private void CleanEvents()
        //{
        //    if (_downloads is not null)
        //        _downloads.Error -= OnError;
        //    if (_installs is not null)
        //        _installs.Error -= OnError;
        //    _elevator.ElevationRequested -= OnElevationRequested;
        //}

        //private void QueueInitialActivities()
        //{
        //    //_installs.Queue(new WaitTask(_downloads)); // Waits until all downloads are finished
        //    _installMutexTask = new AcquireMutexTask(_serviceProvider);
        //    _installs.Queue(_installMutexTask);
        //}

        //private PackageActivities PlanInstallable(ProductComponent productComponent, Dictionary<ProductComponent, ComponentDownloadTask> downloadLookup)
        //{
        //    PackageActivities packageActivities = null;
        //    if (productComponent != null)
        //    {
        //        var isPresent = productComponent.CurrentState == CurrentState.Installed;

        //        if (productComponent.RequiredAction == ComponentAction.Update || 
        //            productComponent.RequiredAction == ComponentAction.Keep)
        //        {
        //            // TODO: Debug this and check if everything is correct!!!!
        //            packageActivities = CreateDownloadInstallActivities(productComponent, productComponent.RequiredAction, isPresent);
        //            if (packageActivities.Install != null)
        //                _itemsToInstall.Add(packageActivities.Install);
        //            if (packageActivities.Download != null)
        //                downloadLookup[productComponent] = packageActivities.Download;
        //        }

        //        if (productComponent.RequiredAction == ComponentAction.Delete)
        //        {
        //            packageActivities = CreateDownloadInstallActivities(productComponent, productComponent.RequiredAction, isPresent);
        //            if (packageActivities.Download != null)
        //                downloadLookup[productComponent] = packageActivities.Download;
        //        }
        //    }
        //    return packageActivities;
        //}

        //private PackageActivities CreateDownloadInstallActivities(ProductComponent productComponent, ComponentAction action, bool isPresent)
        //{
        //    ComponentDownloadTask? downloadTask;
        //    ComponentInstallTask install;

        //    if (DownloadRequired(action, productComponent))
        //    {
        //        downloadTask = new ComponentDownloadTask(_serviceProvider, productComponent, Configuration);
        //        downloadTask.Canceled += (_, __) => _linkedCancellationTokenSource?.Cancel();
        //        install = new ComponentInstallTask(_serviceProvider, productComponent, action, Configuration, downloadTask, isPresent);
        //    }
        //    else
        //    {
        //        downloadTask = null;
        //        install = new ComponentInstallTask(_serviceProvider, productComponent, action, Configuration, isPresent);
        //    }

        //    return new PackageActivities
        //    {
        //        Download = downloadTask,
        //        Install = install
        //    };
        //}

        //private bool DownloadRequired(ComponentAction action, ProductComponent productComponent)
        //{
        //    if (action != ComponentAction.Update)
        //        return false;

        //    if (productComponent.CurrentState == CurrentState.Downloaded && UpdateItemDownloadPathStorage.Instance.TryGetValue(productComponent, out _))
        //        return false;


        //    return true;
        //}

        //private void OnError(object sender, TaskEventArgs e)
        //{
        //    IsCancelled |= e.Cancel;
        //    if (e.Cancel)
        //        _linkedCancellationTokenSource?.Cancel();
        //    try
        //    {
        //        if (e.Cancel || !(e.Task is IComponentTask))
        //            return;

        //        if (e.Task is ComponentInstallTask installTask)
        //        {
        //            if (installTask.Result.IsFailure())
        //            {
        //                // TODO
        //            }
        //            else
        //            {
        //                // TODO
        //            }
        //        }
        //        else if (e.Task is ComponentDownloadTask)
        //        {
        //            // TODO
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Skipping as an error occurred");
        //    }
        //}

        //private class PackageActivities
        //{
        //    internal ComponentDownloadTask? Download { get; init; }

        //    internal ComponentInstallTask Install { get; init; }
        //}
    }
}