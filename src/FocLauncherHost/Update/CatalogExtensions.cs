using System;
using FocLauncherHost.Update.Model;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New;
using TaskBasedUpdater.Verification;
using Requires = Validation.Requires;

namespace FocLauncherHost.Update
{
    internal interface ILauncherCatalogFinder
    {
        LauncherUpdateManifestModel FindMatching(LauncherUpdateManifestContainer container);
    }

    internal class LauncherCatalogFinder : ILauncherCatalogFinder
    {
        public LauncherUpdateManifestModel FindMatching(LauncherUpdateManifestContainer container)
        {
            throw new NotImplementedException();
        }
    }

    internal class LauncherToProductCatalogConverter : ICatalogConverter<LauncherUpdateManifestModel, LauncherComponent>
    {
        public IComponentConverter<LauncherComponent> ComponentConverter { get; }

        public LauncherToProductCatalogConverter() : 
            this(new DependencyToComponentConverter())
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

    internal class DependencyToComponentConverter : IComponentConverter<LauncherComponent>
    {
        public ProductComponent Convert(LauncherComponent dependency)
        {
            Requires.NotNullAllowStructs(dependency, nameof(dependency));
            return null;
        }

        public static ProductComponent? DependencyToComponent(LauncherComponent dependency)
        {
            if (string.IsNullOrEmpty(dependency.Name) || string.IsNullOrEmpty(dependency.Destination))
                return null;

            if (string.IsNullOrEmpty(dependency.Origin))
                return new ProductComponent(dependency.Name, GetRealDependencyDestination(dependency));
            
            var newVersion = dependency.GetVersion();
            var hash = dependency.Sha2;
            var size = dependency.Size;

            var verificationContext = VerificationContext.None;
            if (hash != null)
                verificationContext = new VerificationContext(hash, HashType.Sha256);
            var originInfo = new OriginInfo(new Uri(dependency.Origin, UriKind.Absolute))
            {
                Size = size,
                VerificationContext = verificationContext
            };

            return new ProductComponent(dependency.Name, GetRealDependencyDestination(dependency))
            {
                OriginInfo = originInfo,
                CurrentVersion = newVersion
            };
        }

        private static string GetRealDependencyDestination(LauncherComponent dependency)
        {
            var destination = Environment.ExpandEnvironmentVariables(dependency.Destination);
            if (!Uri.TryCreate(destination, UriKind.Absolute, out var uri))
                throw new InvalidOperationException($"No absolute dependency destination: {destination}");
            return uri.LocalPath;
        }
    }
}