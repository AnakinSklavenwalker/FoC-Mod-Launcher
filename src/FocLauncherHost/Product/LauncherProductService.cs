using System;
using System.IO;
using System.IO.Abstractions;
using FocLauncher;
using FocLauncher.Properties;
using FocLauncher.UpdateMetadata;
using FocLauncher.Xml;
using FocLauncherHost.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Product.Manifest;

namespace FocLauncherHost.Product
{
    internal sealed class LauncherProductService : ProductServiceBase
    {
        private readonly IServiceProvider _serviceProvider;

        public LauncherProductService(IProductComponentBuilder componentBuilder, IServiceProvider serviceProvider) :
            base(componentBuilder, serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override IProductReference CreateProductReference(Version? newVersion,
            ProductReleaseType newReleaseType)
        {
            return new ProductReference(LauncherConstants.ProductName)
            {
                Version = newVersion,
                ReleaseType = newReleaseType
            };
        }

        protected override IInstalledProduct BuildProduct()
        {
            var productRef = CreateProductReference(null, ProductReleaseType.Stable);
            var path = GetInstallationPath();
            var manifest = GetManifest(productRef);
            return new InstalledProduct(productRef, manifest, path);
        }

        protected override IAvailableProductManifest LoadManifest(IProductReference product, IFileInfo manifestFile)
        {
            using var fileStream = manifestFile.OpenRead();
            using var validator = new CatalogValidator(_serviceProvider);

            Logger?.LogTrace($"Validating manifest file {manifestFile.FullName}");
            if (!validator.Validate(fileStream))
                throw new ManifestException($"Manifest file '{manifestFile.FullName}' is not valid.");
            var manifest = Catalogs.FromStreamSafe(fileStream);
            if (manifest is null)
                throw new ManifestException($"Failed to get manifest from '{manifestFile.FullName}'.");
            Logger?.LogTrace($"Validation successful.");


            throw new NotImplementedException();
        }

        protected override bool IsProductCompatible(IProductReference product)
        {
            return ProductReferenceEqualityComparer.NameOnly.Equals(GetCurrentInstance().ProductReference, product);
        }


        private static IInstalledProductManifest GetManifest(IProductReference productReference)
        {
            return new LauncherManifest(productReference);
        }

        private static string GetInstallationPath()
        {
            return Directory.GetCurrentDirectory();
        }
    }

    public class CatalogValidator : IDisposable
    {
        private XmlValidator _validator;

        public CatalogValidator(IServiceProvider? serviceProvider = null)
        {
            var schema = Resources.UpdateValidator.ToStream();
            var logger = serviceProvider?.GetService<ILogger>();
            _validator = new XmlValidator(schema);
        }

        public bool Validate(Stream catalogStream)
        {
            return _validator.Validate(catalogStream).IsValid;
        }

        public void Dispose()
        {
            _validator.Dispose();
            _validator = null;
        }
    }
}