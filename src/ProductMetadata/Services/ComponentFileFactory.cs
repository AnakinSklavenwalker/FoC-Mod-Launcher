using System.IO.Abstractions;
using CommonUtilities;
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
        
        public ProductComponent Create(ProductComponent baseComponent, IProductComponentBuilder builder)
        {
            Requires.NotNull(baseComponent, nameof(baseComponent));
            Requires.NotNull(builder, nameof(builder));


            var realPath = _destinationResolver.GetFullDestination(baseComponent, false, _basePath);

            var filePath = _fileSystem.Path.Combine(realPath, baseComponent.Name);
            var file = _fileSystem.FileInfo.FromFileName(filePath);
            if (!file.Exists)
                return baseComponent with { DetectedState = DetectionState.Absent };

            var version = builder.GetVersion(file);
            var size = file.Length;

            var integrityInformation = ComponentIntegrityInformation.None;
            if (builder.HashType != HashType.None)
            {
                var hashingService = new HashingService();
                var hash = hashingService.GetFileHash(file, builder.HashType);
                integrityInformation = new ComponentIntegrityInformation(hash, builder.HashType);
            }

            return baseComponent with
            {
                Destination = realPath,
                DiskSize = size,
                DetectedState = DetectionState.Present,
                CurrentVersion = version,
                IntegrityInformation = integrityInformation
            };
        }
    }

    public interface IComponentFactory
    {
        ProductComponent Create(ProductComponent baseComponent, IProductComponentBuilder builder);
    }
}