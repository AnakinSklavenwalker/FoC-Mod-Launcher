using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata.Services
{
    public abstract class ComponentDetectorBase : IComponentDetector
    {
        protected ILogger? Logger { get; }

        protected ComponentDetectorBase(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Logger = serviceProvider.GetService<ILogger>();
        }
        
        public IProductComponent Find(IProductComponent manifestComponent, IInstalledProduct product)
        {
            Requires.NotNull(manifestComponent, nameof(manifestComponent));
            Requires.NotNull(product, nameof(product));
            return FindCore();
        }

        protected abstract IProductComponent FindCore();
    }
}