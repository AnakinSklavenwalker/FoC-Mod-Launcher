using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using TaskBasedUpdater.Verification;
using Validation;

namespace TaskBasedUpdater.New.Product.Component
{
    public class ComponentFileFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFullDestinationResolver _destinationResolver;
        private readonly string _basePath;

        public ComponentFileFactory(IServiceProvider serviceProvider, IFullDestinationResolver destinationResolver, string basePath)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(destinationResolver, nameof(destinationResolver));
            Requires.NotNullOrEmpty(basePath, nameof(basePath));
            _serviceProvider = serviceProvider;
            _destinationResolver = destinationResolver;
            _basePath = basePath;
        }
        
        public ProductComponent FromFile(ProductComponent baseComponent, IProductComponentBuilder builder)
        {
            Requires.NotNull(baseComponent, nameof(baseComponent));
            Requires.NotNull(builder, nameof(builder));


            var realPath = _destinationResolver.GetFullDestination(baseComponent, false, _basePath);

            var fs = _serviceProvider.GetRequiredService<IFileSystem>();

            var filePath = fs.Path.Combine(realPath, baseComponent.Name);
            var file = fs.FileInfo.FromFileName(filePath);
            if (!file.Exists)
                return baseComponent with { DetectedState = DetectionState.Absent };

            var version = builder.GetVersion(file);
            var size = file.Length;

            var verificationContext = VerificationContext.None;
            if (builder.HashType != HashType.None)
            {
                var hashingService = new HashingService();
                var hash = hashingService.GetFileHash(file, builder.HashType);
                verificationContext = new VerificationContext(hash, builder.HashType);
            }

            return baseComponent with
            {
                Destination = realPath,
                DiskSize = size,
                DetectedState = DetectionState.Present,
                CurrentVersion = version,
                VerificationContext = verificationContext
            };
        }
    }
}