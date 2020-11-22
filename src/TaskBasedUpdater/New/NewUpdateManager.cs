using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskBasedUpdater.Configuration;
using Validation;

namespace TaskBasedUpdater.New
{
    public sealed class AggregatedServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceProvider _aggregated;
        private readonly Lazy<IServiceProvider> _current;

        public bool IsDisposed { get; private set; }

        public AggregatedServiceProvider(IServiceProvider serviceProvider, Func<IServiceCollection> serviceBuilder)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(serviceBuilder, nameof(serviceBuilder));
            _aggregated = serviceProvider;
            _current = new Lazy<IServiceProvider>(() => CreateServiceProvider(serviceBuilder));
        }

        ~AggregatedServiceProvider()
        {
            Dispose(false);
        }

        public object GetService(Type serviceType)
        {
            Requires.NotNull(serviceType, nameof(serviceType));
            var service = _current.Value?.GetService(serviceType);
            return service ?? _aggregated.GetService(serviceType);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                if (_aggregated is IDisposable disposable)
                    disposable.Dispose();
            }
            IsDisposed = true;
        }

        private static IServiceProvider CreateServiceProvider(Func<IServiceCollection> serviceBuilder)
        {
            var serviceProvider = serviceBuilder().BuildServiceProvider();
            if (serviceProvider is null)
                throw new InvalidOperationException("Service Provider Factory must not return null");
            return serviceProvider;
        }
    }

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