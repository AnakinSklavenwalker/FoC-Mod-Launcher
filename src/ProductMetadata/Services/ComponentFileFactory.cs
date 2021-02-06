using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata.Services
{
    public class ComponentFileFactory : IComponentFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly IProductComponentBuilder _builder;

        public ComponentFileFactory(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
            _builder = serviceProvider.GetRequiredService<IProductComponentBuilder>();
        }
        
        public ProductComponent Create(ProductComponent manifestComponent, IInstalledProduct product)
        {
            Requires.NotNull(manifestComponent, nameof(manifestComponent));
            Requires.NotNull(product, nameof(product));

            // TODO : Remove interface
            var componentPath = _builder.ResolveComponentDestination(manifestComponent, product);

            var file = _fileSystem.FileInfo.FromFileName(componentPath);
            if (!file.Exists)
                return manifestComponent with { DetectedState = DetectionState.Absent };

            var version = _builder.GetVersion(file);
            var size = _builder.GetSize(file);
            var integrityInformation = _builder.GetIntegrityInformation(file);
            

            return manifestComponent with
            {
                Destination = componentPath,
                DiskSize = size,
                DetectedState = DetectionState.Present,
                CurrentVersion = version,
                IntegrityInformation = integrityInformation
            };
        }
    }
}