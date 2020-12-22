using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New.Update;
using Validation;

namespace TaskBasedUpdater.New
{
    public class NewUpdateManager : IUpdateManager
    {
        private readonly IServiceProvider _serviceProvider;

        public IUpdateConfiguration UpdateConfiguration { get; }

        public bool IsDisposed { get; private set; }

        public NewUpdateManager(IServiceProvider serviceProvider, IUpdateConfiguration updateConfiguration)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            _serviceProvider = new AggregatedServiceProvider(serviceProvider, CreateServices);
            UpdateConfiguration = updateConfiguration;
        }
        
        ~NewUpdateManager()
        {
            Dispose(false);
        }
        
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public IUpdateResultInformation Update(IUpdateCatalog updateCatalog, CancellationToken cancellation)
        {
            Task.Run(async () => await Task.Delay(10000, cancellation), cancellation).Wait(cancellation);
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IServiceCollection CreateServices()
        {
            return new ServiceCollection();
        }
    }
}