using System;
using FocLauncherHost.Product;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using TaskBasedUpdater.New.Product;

namespace FocLauncherHost.Update
{
    internal class UpdaterServiceFactory
    {
        public IServiceProvider CreateServiceProvider(IServiceProvider services)
        {
            Requires.NotNull(services, nameof(services));
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IProductService>(sp => new LauncherProductService(new LauncherComponentBuilder(), services));
            return serviceCollection.BuildServiceProvider();
        }
    }
}