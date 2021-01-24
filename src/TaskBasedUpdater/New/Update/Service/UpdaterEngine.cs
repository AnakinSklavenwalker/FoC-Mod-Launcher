using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.Operations;
using Validation;

namespace TaskBasedUpdater.New.Update.Service
{
    internal class UpdaterEngine
    {
        private readonly IServiceProvider _serviceProvider;
        private ILogger? _logger;
        private bool _isInitialized;
        private IFileSystem _fileSystem;
        private readonly object _syncLock = new();

        public UpdateConfiguration UpdateConfiguration { get; }

        public bool IsDisposed { get; private set; }
        
        public bool IsRunning { get; private set; }

        public IInstalledProduct ProductInstance { get; }

        public UpdaterEngine(IInstalledProduct product, IServiceProvider serviceProvider, UpdateConfiguration updateConfiguration)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            _serviceProvider = serviceProvider;
            UpdateConfiguration = updateConfiguration;
            ProductInstance = product;
        }
        
        ~UpdaterEngine()
        {
            Dispose(false);
        }
        
        public void Update(IUpdateCatalog updateCatalog, CancellationToken cancellation = default)
        {
            Requires.NotNull(updateCatalog, nameof(updateCatalog));
            Initialize();
            lock (_syncLock)
            {
                if (IsRunning)
                    throw new InvalidOperationException("This engine has already been used for one update operation and cannot be used for another.");
                IsRunning = true;
            }
            try
            {
                if (!ProductReferenceEqualityComparer.NameOnly.Equals(ProductInstance.ProductReference,
                    updateCatalog.Product))
                    throw new InvalidOperationException("Incompatible products.");
                if (!updateCatalog.Items.Any())
                    throw new InvalidOperationException("Update catalog does not have any items.");
                VerifyInstallationPath(ProductInstance, _fileSystem);
                // Measure drive space before
                var updateOperation = CreateUpdateOperation(ProductInstance, updateCatalog);
                RunPrecheck();
                RunOperation(updateOperation, cancellation);
                // Measure drive space after
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                IsRunning = false;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Initialize()
        {
            if (_isInitialized)
                return;
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UpdaterEngine));
            _logger = _serviceProvider.GetService<ILogger>();
            _fileSystem = _serviceProvider.GetRequiredService<IFileSystem>();
            _isInitialized = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;


            IsDisposed = true;
        }

        private UpdateOperation CreateUpdateOperation(IInstalledProduct product, IUpdateCatalog updateCatalog)
        {
            var operation = new UpdateOperation(product, UpdateConfiguration, updateCatalog.Items, _serviceProvider);
            operation.Schedule();
            return operation;
        }

        private void RunPrecheck()
        {
        }

        private void RunOperation(UpdateOperation updateOperation, CancellationToken cancellation)
        {
            try
            {
                if (UpdateConfiguration.SuspendOsFromSleeping)
                {
                    // TODO: Suspend OS from sleeping
                }

                try
                {
                    _logger?.LogTrace("Starting update");
                    Task.Run(async () => await Task.Delay(10000, cancellation), cancellation).Wait(cancellation);
                    updateOperation.Run(cancellation);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed update: {e.Message}");
                    throw;
                }
                finally
                {
                    // TODO: Clear some locked files list.
                    _logger.LogTrace("Completed update.");
                }
               
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("User canceled during update subsession.");
                throw;
            }
            catch (ComponentFailedException ex)
            {
                _logger?.LogWarning("Package Failed to update");
                throw;
            }
            catch (AggregateException ex)
            {
                if (ex.IsExceptionType<OperationCanceledException>())
                    _logger?.LogWarning("User canceled during update subsession.");
                else
                    _logger?.LogError(ex, "Exception in update operation.");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception in update operation.");
                throw;
            }
        }

        private static void VerifyInstallationPath(IInstalledProduct productInstance, IFileSystem fileSystem)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNull(productInstance, nameof(productInstance));
            Requires.NotNullOrEmpty(productInstance.InstallationPath, nameof(productInstance.InstallationPath));

            var installationPath = Environment.ExpandEnvironmentVariables(productInstance.InstallationPath);
            if (!fileSystem.IsValidDirectoryPath(installationPath) ||
                !fileSystem.Path.IsPathRooted(installationPath) ||
                fileSystem.File.Exists(installationPath) ||
                fileSystem.IsOpticalDevice(installationPath) ||
                !fileSystem.IsDriveFixedOrRemovable(installationPath))
                throw new InvalidOperationException($"The path '{installationPath}' is invalid.");
        }
    }
}