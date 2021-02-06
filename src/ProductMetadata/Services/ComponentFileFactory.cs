using System.IO.Abstractions;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata.Services
{
    public class ComponentFileFactory : IComponentFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFullDestinationResolver _destinationResolver;
        private readonly string _basePath;

        public ComponentFileFactory(IFullDestinationResolver destinationResolver, IFileSystem fileSystem, string basePath)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNull(destinationResolver, nameof(destinationResolver));
            Requires.NotNullOrEmpty(basePath, nameof(basePath));
            _fileSystem = fileSystem;
            _destinationResolver = destinationResolver;
            _basePath = basePath;
        }
        
        public ProductComponent Create(ProductComponent manifestComponent, IProductComponentBuilder builder)
        {
            Requires.NotNull(manifestComponent, nameof(manifestComponent));
            Requires.NotNull(builder, nameof(builder));

            // TODO : Remove interface
            var realPath = _destinationResolver.GetFullDestination(manifestComponent, false, _basePath);

            var filePath = _fileSystem.Path.Combine(realPath, manifestComponent.Name);
            var file = _fileSystem.FileInfo.FromFileName(filePath);
            if (!file.Exists)
                return manifestComponent with { DetectedState = DetectionState.Absent };

            var version = builder.GetVersion(file);
            var size = builder.GetSize(file);
            var integrityInformation = builder.GetIntegrityInformation(file);
            

            return manifestComponent with
            {
                Destination = realPath,
                DiskSize = size,
                DetectedState = DetectionState.Present,
                CurrentVersion = version,
                IntegrityInformation = integrityInformation
            };
        }
    }
}