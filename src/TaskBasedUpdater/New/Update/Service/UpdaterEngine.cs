﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Configuration;
using Validation;

namespace TaskBasedUpdater.New.Update.Service
{
    internal class UpdaterEngine
    {
        private readonly IServiceProvider _serviceProvider;
        private ILogger? _logger;
        private bool _isInitialized;

        public UpdateConfiguration UpdateConfiguration { get; }

        public bool IsDisposed { get; private set; }
        
        
        public bool IsRunning { get; set; }

        public UpdaterEngine(IServiceProvider serviceProvider, UpdateConfiguration updateConfiguration)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            _serviceProvider = new AggregatedServiceProvider(serviceProvider, CreateServices);
            UpdateConfiguration = updateConfiguration;
        }
        
        ~UpdaterEngine()
        {
            Dispose(false);
        }
        
        public void Initialize()
        {
            if (_isInitialized)
                return;
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UpdaterEngine));

            _logger = _serviceProvider.GetService<ILogger>();


            _isInitialized = true;
        }

        public void Update(IUpdateCatalog updateCatalog, CancellationToken cancellation = default)
        {
            Requires.NotNull(updateCatalog, nameof(updateCatalog));
            Initialize();
            try
            {

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

            Task.Run(async () => await Task.Delay(10000, cancellation), cancellation).Wait(cancellation);
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