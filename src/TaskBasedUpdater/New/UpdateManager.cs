using System;
using System.Linq;
using System.Threading;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Update;
using Validation;

namespace TaskBasedUpdater.New
{
    public class UpdateService
    {
        //InstallerBase
        //InstallerService
        //ProductInstaller
    }

    public class UpdateManager : IDisposable
    {
        private readonly IProductService _productService;
        private readonly IServiceProvider _services;
        private IUpdateCatalog? _updateCatalog;
        private UpdaterEngine? _updateManager; // TODO: split-projects user interface

        public UpdateConfiguration UpdateConfiguration { get; }

        public UpdateManager(IProductService productService, UpdateConfiguration updateConfiguration,
            IServiceProvider services)
        {
            Requires.NotNull(productService, nameof(productService));
            Requires.NotNull(services, nameof(services));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            _productService = productService;
            _services = services;
            UpdateConfiguration = updateConfiguration;
        }
        
        public UpdateResultInformation Update(CancellationToken token)
        {
            _updateManager = new UpdaterEngine(_services, UpdateConfiguration);

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