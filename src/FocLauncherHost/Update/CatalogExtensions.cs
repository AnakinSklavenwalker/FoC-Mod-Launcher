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
        ProductCatalog FindMatching(Catalogs container);
    }

    internal class LauncherCatalogFinder : ILauncherCatalogFinder
    {
        public ProductCatalog FindMatching(Catalogs container)
        {
            throw new NotImplementedException();
        }
    }

    internal class LauncherToProductCatalogConverter : ICatalogConverter<ProductCatalog, Dependency>
    {
        public IComponentConverter<Dependency> ComponentConverter { get; }

        public LauncherToProductCatalogConverter() : 
            this(new DependencyToComponentConverter())
        {
            
        }

        internal LauncherToProductCatalogConverter(IComponentConverter<Dependency> componentConverter)
        {
            Requires.NotNull(componentConverter, nameof(componentConverter));
            ComponentConverter = componentConverter;
        }

        public ICatalog Convert(ProductCatalog catalogModel)
        {
            throw new NotImplementedException();
        }
    }

    internal class DependencyToComponentConverter : IComponentConverter<Dependency>
    {
        public ProductComponent Convert(Dependency dependency)
        {
            Requires.NotNullAllowStructs(dependency, nameof(dependency));
            return null;
        }

        public static ProductComponent? DependencyToComponent(Dependency dependency)
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

        private static string GetRealDependencyDestination(Dependency dependency)
        {
            var destination = Environment.ExpandEnvironmentVariables(dependency.Destination);
            if (!Uri.TryCreate(destination, UriKind.Absolute, out var uri))
                throw new InvalidOperationException($"No absolute dependency destination: {destination}");
            return uri.LocalPath;
        }
    }
}