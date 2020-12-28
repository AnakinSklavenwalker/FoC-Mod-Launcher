using System;
using System.Linq;
using System.Threading;
using Microsoft;
using TaskBasedUpdater;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Update;

namespace FocLauncherHost.Update
{
    internal class FocLauncherUpdater : IDisposable
    {
        private readonly IProductService _productService;
        private readonly IServiceProvider _services;
        private IUpdateCatalog? _updateCatalog;
        private IUpdateManager? _updateManager;

        public UpdateConfiguration UpdateConfiguration { get; }

        public FocLauncherUpdater(IProductService productService, UpdateConfiguration updateConfiguration,
            IServiceProvider services)
        {
            Requires.NotNull(productService, nameof(productService));
            Requires.NotNull(services, nameof(services));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            _productService = productService;
            _services = services;
            UpdateConfiguration = updateConfiguration;
        }

        public UpdateResultInformation CheckAndUpdate(UpdateRequest updateRequest, CancellationToken token)
        {
            Requires.NotNull(updateRequest, nameof(updateRequest));
            try
            {
                if (!IsUpdateAvailable(updateRequest))
                    return UpdateResultInformation.NoUpdate;
                if (_updateCatalog is null)
                    return new UpdateResultInformation
                    {
                        Result = UpdateResult.NoUpdate,
                        Message = "Unable to find update manifest."
                    };
                token.ThrowIfCancellationRequested();
                return Update(token);
            }
            catch (OperationCanceledException)
            {
                return new UpdateResultInformation
                {
                    Result = UpdateResult.Cancelled,
                    Message = "Update cancelled by user request."
                };
            }
        }

        public bool IsUpdateAvailable(UpdateRequest updateRequest)
        {
            var productProviderService = _productService;

            var currentCatalog = productProviderService.GetInstalledProductCatalog();
            var availableCatalog = productProviderService.GetAvailableProductCatalog(updateRequest);

            IUpdateCatalogBuilder builder = new UpdateCatalogBuilder();
            var updateCatalog = builder.Build(currentCatalog, availableCatalog);

            if (!updateCatalog.Items.Any())
                return false;
            _updateCatalog = updateCatalog;
            return true;
        }

        private UpdateResultInformation Update(CancellationToken token)
        {
            _updateManager = new NewUpdateManager(_services, UpdateConfiguration);

            if (_updateCatalog is null)
                throw new InvalidOperationException("Catalog cannot be null");

            _updateManager.Update(_updateCatalog, token);

            return UpdateResultInformation.Success;
        }
        
        public void Dispose()
        {
            _updateManager?.Dispose();
        }
    }
}