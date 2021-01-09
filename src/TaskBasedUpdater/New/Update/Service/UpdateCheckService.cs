using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.New.Product;
using Validation;

namespace TaskBasedUpdater.New.Update.Service
{
    public class UpdateCheckService : IUpdateCheckService
    {
        private readonly IProductService _productService;
        private readonly object _syncObject = new();
        private CancellationTokenSource? _updateCheckToken;
        private readonly ILogger? _logger;

        public bool IsCheckingForUpdates
        {
            get
            {
                lock (_syncObject)
                    return _updateCheckToken != null;
            }
        }

        // TODO: split-projects remove sp from ctor and have custom object
        public UpdateCheckService(IProductService productService, IServiceProvider serviceProvider)
        {
            Requires.NotNull(productService, nameof(productService));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _productService = productService;
            _logger = serviceProvider.GetService<ILogger>();
        }

        public async Task<UpdateCheckResult> CheckForUpdates(UpdateRequest updateRequest, CancellationToken token = default)
        {
            Requires.NotNull(updateRequest, nameof(updateRequest));
            lock (_syncObject)
            {
                if (IsCheckingForUpdates)
                    return UpdateCheckResult.AlreadyInProgress;
                _updateCheckToken = new CancellationTokenSource();
            }

            try
            {
                var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                var updateCatalog = await CheckForUpdatesInternalAsync(updateRequest, linkedTokenSource.Token).ConfigureAwait(false);
                return UpdateCheckResult.Succeeded(updateCatalog);
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                return UpdateCheckResult.Cancelled;
            }
            catch (Exception e)
            {
                return UpdateCheckResult.FromError(e);
            }
            finally
            {
                lock (_syncObject)
                {
                    _updateCheckToken.Dispose();
                    _updateCheckToken = null;
                }
            }
        }

        private Task<IUpdateCatalog>
            CheckForUpdatesInternalAsync(UpdateRequest updateRequest, CancellationToken token) => Task.Run(
            () =>
            {
                ValidateRequest(updateRequest);
                var currentCatalog = _productService.GetInstalledProductCatalog();
                var availableCatalog = _productService.GetAvailableProductCatalog(updateRequest);
                
                // TODO: split-projects use from services.
                IUpdateCatalogBuilder builder = new UpdateCatalogBuilder();
                return builder.Build(currentCatalog, availableCatalog);
            }, token);

        private void ValidateRequest(UpdateRequest updateRequest)
        {
            if (updateRequest.Product is null)
            {
                InvalidOperationException operationException = new("The product reference for an update request must be set.");
                _logger?.LogError(operationException, operationException.Message);
                throw operationException;
            }

            var manifestUri = updateRequest.UpdateManifestPath;
            if (manifestUri is null)
            {
                InvalidOperationException operationException = new("The manifest uri of an update request must be set.");
                _logger?.LogError(operationException, operationException.Message);
                throw operationException;
            }
            if (!manifestUri.IsAbsoluteUri)
            {
                InvalidOperationException operationException = new("The manifest uri : " + manifestUri.AbsoluteUri + " needs to be absolute.");
                _logger?.LogError(operationException, operationException.Message);
                throw operationException;
            }
        }
    }
}