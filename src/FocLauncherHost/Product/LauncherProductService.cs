using System;
using System.IO;
using System.IO.Abstractions;
using FocLauncher;
using FocLauncher.Properties;
using FocLauncher.Xml;
using FocLauncherHost.Update.Model;
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

            if (!validator.Validate(fileStream))
                throw new ManifestException($"Manifest file '{manifestFile.FullName}' is not valid.");
            LauncherUpdateManifestContainer manifestContainer;
            try
            {
                manifestContainer = LauncherUpdateManifestContainer.FromStream(fileStream);
            }
            catch (Exception e)
            {
                throw new ManifestException(e.Message, e);
            }
            if (manifestContainer is null)
                throw new ManifestException($"Failed to get manifest from '{manifestFile.FullName}'.");
            

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
        private ILogger? _logger;

        public CatalogValidator(IServiceProvider? serviceProvider = null)
        {
            var schema = Resources.UpdateValidator.ToStream();
            _logger = serviceProvider?.GetService<ILogger>();
            _validator = new XmlValidator(schema);
        }

        public bool Validate(Stream catalogStream)
        {
            var fileName = (catalogStream as FileStream)?.Name;
            _logger?.LogTrace($"Validating manifest file {fileName}");
            var result = _validator.Validate(catalogStream).IsValid;
            LogResult(result);
            return result;
        }

        private void LogResult(bool result)
        {
            var resultState = result ? "successful" : "failed";
            var message = $"Validation {resultState}";
            if (result)
                _logger?.LogTrace(message);
            else
                _logger?.LogWarning(message);
        }

        public void Dispose()
        {
            _validator.Dispose();
            _validator = null;
            _logger = null;
        }
    }
}