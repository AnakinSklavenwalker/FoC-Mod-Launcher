using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.New.Product.Manifest;
using TaskBasedUpdater.New.Update;
using Validation;

namespace TaskBasedUpdater.New.Product
{
    public abstract class ProductServiceBase : IProductService
    {
        private readonly IServiceProvider _serviceProvider;
        private bool _isInitialized;
        private IInstalledProduct? _installedProduct;

        protected ILogger? Logger;

        protected IProductComponentBuilder ComponentBuilder { get; }

        protected IAvailableManifestBuilder AvailableManifestBuilder { get; }
        
        protected ProductServiceBase(IProductComponentBuilder componentBuilder, IAvailableManifestBuilder availableManifestBuilder, IServiceProvider serviceProvider)
        {
            Requires.NotNull(componentBuilder, nameof(componentBuilder));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(availableManifestBuilder, nameof(availableManifestBuilder));
            ComponentBuilder = componentBuilder;
            AvailableManifestBuilder = availableManifestBuilder;
            _serviceProvider = serviceProvider;
        }

        public IInstalledProduct GetCurrentInstance()
        {
            Initialize();
            return _installedProduct!;
        }

        public void UpdateCurrentInstance(IInstalledProduct product)
        {
            throw new NotImplementedException();
        }

        public abstract IProductReference CreateProductReference(Version? newVersion, ProductReleaseType newReleaseType);

        public IInstalledProductCatalog GetInstalledProductCatalog()
        {
            Initialize();
            var manifest = _installedProduct!.ProductManifest;
            var installPath = _installedProduct.InstallationPath;
            return new InstalledProductCatalog(_installedProduct!, FindInstalledComponents(manifest, installPath));
        }

        public IAvailableProductManifest GetAvailableProductCatalog(UpdateRequest updateRequest)
        {
            Initialize();
            if (!IsProductCompatible(updateRequest.Product))
                throw new InvalidOperationException("Not compatible product");

            var engine = new ManifestDownloadEngine(_serviceProvider);
            var manifestFile = engine.DownloadManifest(updateRequest.UpdateManifestPath);

            try
            {
                Logger?.LogInformation($"Loading manifest form {manifestFile.FullName}");
                IAvailableProductManifest manifest = LoadManifest(updateRequest.Product, manifestFile);
                if (manifest is null)
                    throw new InvalidOperationException("Manifest must not be null");
                return manifest;
            }
            catch (Exception e)
            {
                Logger?.LogError(e, e.Message);
                throw;
            }
            finally
            {
                var fileSystem = _serviceProvider.GetRequiredService<IFileSystem>();
                fileSystem.DeleteFileIfInTemp(manifestFile);
            }
        }

        protected abstract IInstalledProduct BuildProduct();

        protected virtual IEnumerable<ProductComponent> FindInstalledComponents(IInstalledProductManifest manifest, string installationPath)
        {
            foreach (var component in manifest.Items)
            {
                var path = component.GetFilePath();
                path = Path.IsPathRooted(path) ? path : Path.Combine(installationPath, path);
                yield return new ComponentFileFactory(_serviceProvider).FromFile(component, path, ComponentBuilder);
            }
        }

        protected virtual bool IsProductCompatible(IProductReference product)
        {
            return !ProductReferenceEqualityComparer.ReleaseAware.Equals(_installedProduct!.ProductReference, product);
        }
        
        protected virtual IAvailableProductManifest LoadManifest(IProductReference product, IFileInfo manifestFile)
        {
            return AvailableManifestBuilder.Build(product, manifestFile);
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;
            Logger = _serviceProvider.GetService<ILogger>();
            _installedProduct ??= BuildProduct();
            if (_installedProduct is null)
                throw new InvalidOperationException("Created Product must not be null!");
            _isInitialized = true;
        }
    }
}