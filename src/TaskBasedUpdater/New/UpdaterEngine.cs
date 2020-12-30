﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New.Update;
using Validation;

namespace TaskBasedUpdater.New
{
    public class UpdaterEngine
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdateConfiguration UpdateConfiguration { get; }

        public bool IsDisposed { get; private set; }

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