using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata.Services.Detectors
{
    public abstract class ComponentDetectorBase : IComponentDetector
    {
        protected IServiceProvider ServiceProvider { get; }

        protected ILogger? Logger { get; }

        protected abstract ComponentType SupportedType { get; }

        protected ComponentDetectorBase(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetService<ILogger>();
        }
        
        public IProductComponent Find(IProductComponent manifestComponent, IInstalledProduct product)
        {
            Requires.NotNull(manifestComponent, nameof(manifestComponent));
            Requires.NotNull(product, nameof(product));
            ValidateSupported(manifestComponent.Type);
            return FindCore(manifestComponent, product);
        }

        protected abstract IProductComponent FindCore(IProductComponent manifestComponent, IInstalledProduct product);

        protected void ValidateSupported(ComponentType type)
        {
            if (type != SupportedType)
                throw new InvalidOperationException();
        }
    }
}