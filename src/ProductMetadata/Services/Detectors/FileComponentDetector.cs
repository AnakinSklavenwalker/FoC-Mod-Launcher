using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ProductMetadata.Component;

namespace ProductMetadata.Services.Detectors
{
    public sealed class FileComponentDetector : ComponentDetectorBase
    {
        protected override ComponentType SupportedType => ComponentType.File;

        public FileComponentDetector(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override IProductComponent FindCore(IProductComponent manifestComponent, IInstalledProduct product)
        {
            if (manifestComponent is not SingleFileComponent fileComponent)
                throw new InvalidOperationException($"Component {manifestComponent.GetType().Name} is of wrong type. " +
                                                    $"Expected {nameof(SingleFileComponent)}");

            if (fileComponent.OriginInfos.Count != 1)
                throw new InvalidOperationException("SingleFile component must have only one origin info");

            var variableResolver = ServiceProvider.GetService<IVariableResolver>() ?? VariableResolver.Default;

            var fileSystem = ServiceProvider.GetRequiredService<IFileSystem>();

            var originInfo = fileComponent.OriginInfos[0];
            var fileToDetect = fileSystem.Path.Combine(fileComponent.Path, originInfo.FileName);

            var filePath = variableResolver.ResolveVariables(fileToDetect, product.ProductVariables.ToDictionary());

            var detectionState = DetectionState.Absent;
            IList<FileItem> files;
            if (fileSystem.File.Exists(filePath))
            {
                files = new FileItem[] {new(filePath, originInfo.IntegrityInformation)};
                detectionState = DetectionState.Present;
            }
            else
                files = ImmutableList<FileItem>.Empty;


            var detectedFileComponent = new SingleFileComponent(manifestComponent, fileComponent.Path, files)
            {
                DetectedState = detectionState
            };
            return detectedFileComponent;
        }
    }
}