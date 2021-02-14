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
            if (!(manifestComponent is SingleFileComponent fileComponent))
                throw new NotSupportedException();

            if (fileComponent.OriginInfos.Count != 1)
                throw new InvalidOperationException("SingleFile component must have only one origin info");

            var variableResolver = ServiceProvider.GetService<IVariableResolver>() ?? VariableResolver.Default;

            var fileSystem = ServiceProvider.GetRequiredService<IFileSystem>();
            var fileToDetect = fileSystem.Path.Combine(fileComponent.Path, fileComponent.OriginInfos[0].FileName);

            var filePath = variableResolver.ResolveVariables(fileToDetect, product.ProductVariables.ToDictionary());



            FileItem? file = null;

            IList<FileItem> files;
            if (file is null)
                files = ImmutableList<FileItem>.Empty;
            else
                files = new List<FileItem>(1) {file};


            var detectedFileComponent = new SingleFileComponent(manifestComponent, fileComponent.Path, files);
            return detectedFileComponent;
        }
    }
}