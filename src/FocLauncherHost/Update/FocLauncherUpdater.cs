using System;
using System.Linq;
using System.Threading;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using TaskBasedUpdater;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Update;

namespace FocLauncherHost.Update
{
    internal class FocLauncherUpdater : IDisposable
    {
        private readonly IInstalledProduct _product;
        private readonly IServiceProvider _services;
        private IUpdateCatalog? _updateCatalog;
        private IUpdateManager? _updateManager;

        public UpdateConfiguration UpdateConfiguration { get; }

        public FocLauncherUpdater(IInstalledProduct product, UpdateConfiguration updateConfiguration, IServiceProvider services)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(services, nameof(services));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            _product = product;
            _services = new AggregatedServiceProvider(services, InitializeServices);
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

        private UpdateResultInformation Update(CancellationToken token)
        {
            var updater =
                new NewUpdateManager(new ServiceCollection().BuildServiceProvider(), UpdateConfiguration);

            if (_updateCatalog is null)
                throw new InvalidOperationException("Catalog cannot be null");

            updater.Update(_updateCatalog, token);

            return UpdateResultInformation.Success;
        }

        private bool IsUpdateAvailable(UpdateRequest updateRequest)
        {
            var productProviderService = _services.GetRequiredService<IProductService>();

            var currentCatalog = productProviderService.GetInstalledProductCatalog();
            var availableCatalog = productProviderService.GetAvailableProductCatalog(updateRequest);

            IUpdateCatalogBuilder builder = _services.GetRequiredService<IUpdateCatalogBuilder>();
            var updateCatalog = builder.Build(currentCatalog, availableCatalog);

            if (!updateCatalog.Items.Any())
                return false;
            _updateCatalog = updateCatalog;
            return true;
        }

        private IServiceCollection InitializeServices()
        {
            var sc = new ServiceCollection();
            sc.AddTransient<IUpdateCatalogBuilder>(_ => new UpdateCatalogBuilder());
            return sc;
        }

        public void Dispose()
        {
            _updateManager?.Dispose();
        }
    }
}