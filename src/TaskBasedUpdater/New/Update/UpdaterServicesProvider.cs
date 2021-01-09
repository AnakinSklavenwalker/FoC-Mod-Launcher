using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Restart;

namespace TaskBasedUpdater.New.Update
{
    internal class UpdaterServicesProvider : IUpdaterServices, IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public IRestartNotificationService RestartNotificationService =>
            _serviceProvider.GetService<IRestartNotificationService>() ?? new RestartNotificationService();

        public IFileSystem FileSystem => _serviceProvider.GetService<IFileSystem>() ?? new System.IO.Abstractions.FileSystem();
        public ILogger? Logger => _serviceProvider.GetService<ILogger>();

        public UpdaterServicesProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public UpdaterServicesProvider()
        {
            _serviceProvider = new ServiceCollection().BuildServiceProvider();
        }

        internal static IServiceProvider ToServiceProvider(IUpdaterServices services)
        {
            var serviceProvider = CreateServiceProvider(services);
            return new UpdaterServicesProvider(serviceProvider);
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