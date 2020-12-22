using System;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace TaskBasedUpdater.New
{
    public sealed class AggregatedServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceProvider _aggregated;
        private readonly bool _disposeAggregated;
        private readonly Lazy<IServiceProvider> _current;

        public bool IsDisposed { get; private set; }

        public AggregatedServiceProvider(IServiceProvider serviceProvider, Func<IServiceCollection> serviceBuilder, bool disposeAggregated = false)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(serviceBuilder, nameof(serviceBuilder));
            _aggregated = serviceProvider;
            _disposeAggregated = disposeAggregated;
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
                if (_disposeAggregated && _aggregated is IDisposable disposable)
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
}