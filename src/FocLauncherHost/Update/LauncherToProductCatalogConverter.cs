using System;
using System.Linq;
using FocLauncherHost.Update.Model;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New;
using Validation;

namespace FocLauncherHost.Update
{
    internal class LauncherToProductCatalogConverter : ICatalogConverter<LauncherUpdateManifestModel, LauncherComponent>
    {
        public IComponentConverter<LauncherComponent> ComponentConverter { get; }

        public LauncherToProductCatalogConverter(IServiceProvider serviceProvider) : 
            this(new LauncherComponentConverter(new ComponentFullDestinationResolver(serviceProvider)))
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