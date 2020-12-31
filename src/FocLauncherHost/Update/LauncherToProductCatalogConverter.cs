using System;
using FocLauncherHost.Update.Model;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New;
using Validation;

namespace FocLauncherHost.Update
{
    internal class LauncherToProductCatalogConverter : ICatalogConverter<LauncherUpdateManifestModel, LauncherComponent>
    {
        public IComponentConverter<LauncherComponent> ComponentConverter { get; }

        public LauncherToProductCatalogConverter() : 
            this(new LauncherComponentConverter())
        {
        }

        internal LauncherToProductCatalogConverter(IComponentConverter<LauncherComponent> componentConverter)
        {
            Requires.NotNull(componentConverter, nameof(componentConverter));
            ComponentConverter = componentConverter;
        }

        public ICatalog Convert(LauncherUpdateManifestModel catalogModel)
        {
            throw new NotImplementedException();
        }
    }
}