using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncherHost
{
    internal class LauncherServiceFactory
    {
        public IServiceProvider CreateLauncherServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IFileSystem>(_ => new FileSystem());
            return serviceCollection.BuildServiceProvider();
        }
    }
}