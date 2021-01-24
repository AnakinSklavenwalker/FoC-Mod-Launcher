using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.Restart;
using Validation;

namespace TaskBasedUpdater.New.Update.Service
{
    public class UpdateService : IDisposable
    {
        private readonly IInstalledProduct _product;
        private readonly IServiceProvider _services;
        private bool _initialized;
        private readonly object _syncRoot = new();

        public bool UpdateRunning => Engine.IsRunning;

        public bool IsDisposed { get; private set; }

        private UpdaterEngine Engine { get; }

        private ILogger? Logger => _services.GetService<ILogger>();


        public UpdateService(IInstalledProduct product, UpdateConfiguration updateConfiguration) 
            : this(product, updateConfiguration, new UpdaterServicesProvider(updateConfiguration))
        {
        }

        public UpdateService(IInstalledProduct product, UpdateConfiguration updateConfiguration, IUpdaterServices services)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            Requires.NotNull(services, nameof(services));
            _services = UpdaterServicesProvider.ToServiceProvider(services, updateConfiguration);
            Engine = new UpdaterEngine(product, _services, updateConfiguration);
        }

        ~UpdateService() => Dispose(false);

        public async Task<UpdateOperationResult> UpdateAsync(IUpdateCatalog updateCatalog, CancellationToken token)
        {
            try
            {
                if (UpdateRunning)
                    throw new InvalidOperationException("Update already running.");
                Initialize();
                return !updateCatalog.RequiresUpdate() 
                    ? CreateResult(updateCatalog.Product, UpdateResult.NoUpdate) 
                    : CreateResult(await UpdateCoreAsync(updateCatalog, token));
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Updater threw an exception: " + ex.Message);
                return CreateResult(updateCatalog.Product, ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private Task<IProductReference> UpdateCoreAsync(IUpdateCatalog updateCatalog, CancellationToken token)
        {
            return Task.Run(() =>
            {
                if (updateCatalog.Product == null)
                    throw new InvalidOperationException("Failed to get the product reference");
                Engine.Update(updateCatalog, token);
                return updateCatalog.Product;
            }, token);
        }


        private UpdateOperationResult CreateResult(IProductReference product, Exception? exception = null)
        {
            var cancelled = exception != null && exception.IsExceptionType<OperationCanceledException>();
            var requiresRestart = _services.GetRequiredService<IRestartNotificationService>().RestartRequired;

            if (cancelled)
                return CreateResult(product, UpdateResult.Cancelled, exception);
            if (exception is not null)
                return CreateResult(product, UpdateResult.Failed, exception);
            return CreateResult(product, requiresRestart 
                ? UpdateResult.SuccessRestartRequired 
                : UpdateResult.Success, exception);
        }

        private static UpdateOperationResult CreateResult(IProductReference product, UpdateResult result, Exception? exception = null)
        {
            return new(product)
            {
                Result = result,
                Error = exception
            };
        }


        private void Initialize()
        {
            if (_initialized)
                return;
            lock (_syncRoot)
            {
                if (_initialized)
                    return;
                _initialized = true;
                Engine.Initialize();
            }
        }

        protected void Dispose(bool disposing)
        {
            if (this.IsDisposed)
                return;
            if (disposing)
                Engine.Dispose();
            IsDisposed = true;
        }
    }
}