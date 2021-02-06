using System;
using System.IO.Abstractions;
using System.Linq;
using FocLauncherHost.Update.Model;
using ProductMetadata;
using ProductMetadata.Component;
using ProductMetadata.Services;
using Validation;

namespace FocLauncherHost.Update
{
    internal class LauncherToProductCatalogConverter : ICatalogConverter<LauncherUpdateManifestModel, LauncherComponent>
    {
        public IComponentConverter<LauncherComponent> ComponentConverter { get; }

        public LauncherToProductCatalogConverter(IFileSystem fileSystem) : 
            this(new LauncherComponentConverter(new ComponentFullDestinationResolver(fileSystem)))
        {
        }
        
        internal LauncherToProductCatalogConverter(IComponentConverter<LauncherComponent> componentConverter)
        {
            Requires.NotNull(componentConverter, nameof(componentConverter));
            ComponentConverter = componentConverter;
        }

        public ICatalog Convert(LauncherUpdateManifestModel catalogModel)
        {
            Requires.NotNull(catalogModel, nameof(catalogModel));
            var components = catalogModel.Components.Select(c => ComponentConverter.Convert(c));
            return new Catalog(components);
        }
    }
}