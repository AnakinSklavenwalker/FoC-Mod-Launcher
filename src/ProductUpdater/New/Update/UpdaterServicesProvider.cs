using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductUpdater.Configuration;
using ProductUpdater.Restart;
using SimpleDownloadManager;

namespace ProductUpdater.New.Update
{
    internal class UpdaterServicesProvider : IUpdaterServices, IServiceProvider
    {
        public UpdateConfiguration UpdateConfiguration { get; }
        private readonly IServiceProvider _serviceProvider;

        public IRestartNotificationService RestartNotificationService =>
            _serviceProvider.GetService<IRestartNotificationService>() ?? new RestartNotificationService();

        public IFileSystem FileSystem => _serviceProvider.GetService<IFileSystem>() ?? new System.IO.Abstractions.FileSystem();
        public ILogger? Logger => _serviceProvider.GetService<ILogger>();

        public IDownloadManager DownloadManager =>
            _serviceProvider.GetService<IDownloadManager>() ??
            new DownloadManager(UpdateConfiguration.DownloadConfiguration);

        private UpdaterServicesProvider(IServiceProvider serviceProvider, UpdateConfiguration updateConfiguration)
        {
            UpdateConfiguration = updateConfiguration;
            _serviceProvider = serviceProvider;
        }

        public UpdaterServicesProvider(UpdateConfiguration updateConfiguration) 
            : this(new ServiceCollection().BuildServiceProvider(), updateConfiguration)
        {
        }

        internal static IServiceProvider ToServiceProvider(IUpdaterServices services, UpdateConfiguration updateConfiguration)
        {
            var serviceProvider = CreateServiceProvider(services);
            return new UpdaterServicesProvider(serviceProvider, updateConfiguration);
        }


        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
        
        private static IServiceProvider CreateServiceProvider(IUpdaterServices services)
        {
            var serviceCollection = new ServiceCollection();
            AddExistingService<ILogger>(serviceCollection, services.Logger);
            AddExistingService<IFileSystem>(serviceCollection, services.FileSystem);
            AddExistingService<IRestartNotificationService>(serviceCollection, services.RestartNotificationService);
            AddExistingService<IDownloadManager>(serviceCollection, services.DownloadManager);
            return serviceCollection.BuildServiceProvider();
        }

        private static void AddExistingService<T>(IServiceCollection serviceCollection, object? service) where T: class
        {
            if (service is null)
                return;
            serviceCollection.AddSingleton((T)service);
        }
    }
}