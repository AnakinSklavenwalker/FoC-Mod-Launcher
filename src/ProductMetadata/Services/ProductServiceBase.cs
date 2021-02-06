using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using CommonUtilities;
using Microsoft.Extensions.Logging;
using ProductMetadata.Component;
using ProductMetadata.Manifest;
using Validation;

namespace ProductMetadata.Services
{
    public abstract class ProductServiceBase : IProductService
    {
        private bool _isInitialized;
        private IInstalledProduct? _installedProduct;

        protected ILogger? Logger;
        protected readonly IFileSystem FileSystem;

        protected IProductComponentBuilder ComponentBuilder { get; }

        protected IAvailableManifestBuilder AvailableManifestBuilder { get; }
        protected IManifestFileResolver ManifestFileResolver { get; }

        protected IComponentFactory InstalledComponentFactory { get; }

        protected ProductServiceBase(
            IProductComponentBuilder componentBuilder,
            IManifestFileResolver manifestFileResolver,
            IAvailableManifestBuilder availableManifestBuilder,
            IComponentFactory installedComponentFactory,
            IFileSystem fileSystem,
            ILogger? logger = null)
        {
            Requires.NotNull(componentBuilder, nameof(componentBuilder));
            Requires.NotNull(installedComponentFactory, nameof(installedComponentFactory));
            Requires.NotNull(manifestFileResolver, nameof(manifestFileResolver));
            Requires.NotNull(availableManifestBuilder, nameof(availableManifestBuilder));
            ComponentBuilder = componentBuilder;
            ManifestFileResolver = manifestFileResolver;
            AvailableManifestBuilder = availableManifestBuilder;
            InstalledComponentFactory = installedComponentFactory;
            FileSystem = fileSystem;
            Logger = logger;
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

        public IAvailableProductManifest GetAvailableProductManifest(ProductManifestLocation manifestLocation)
        {
            Initialize();
            if (!IsProductCompatible(manifestLocation.Product))
                throw new InvalidOperationException("Not compatible product");
            
            try
            {
                Logger?.LogTrace("Getting manifest file.");
                var manifestFile = GetAvailableManifestFile(manifestLocation);
                if (manifestFile is null || !manifestFile.Exists)
                    throw new ManifestNotFoundException("Manifest file not found or null");
                try
                {
                    Logger?.LogTrace($"Loading manifest form {manifestFile.FullName}");
                    IAvailableProductManifest manifest = LoadManifest(manifestLocation, manifestFile);
                    if (manifest is null)
                        throw new ManifestException("Manifest cannot be null");
                    return manifest;
                }
                finally
                {
                    FileSystem.DeleteFileIfInTemp(manifestFile);
                }
            }
            catch (Exception e)
            {
                Logger?.LogError(e, e.Message);
                throw;
            }
        }

        protected abstract IInstalledProduct BuildProduct();

        protected virtual IEnumerable<ProductComponent> FindInstalledComponents(IInstalledProductManifest manifest, string installationPath)
        {
            return manifest.Items.Select(component => InstalledComponentFactory.Create(component, ComponentBuilder));
        }

        protected virtual IFileInfo GetAvailableManifestFile(ProductManifestLocation manifestLocation)
        {
            return ManifestFileResolver.GetManifest(manifestLocation.UpdateManifestPath);
        }
        
        protected virtual bool IsProductCompatible(IProductReference product)
        {
            return !ProductReferenceEqualityComparer.ReleaseAware.Equals(_installedProduct!.ProductReference, product);
        }
        
        protected virtual IAvailableProductManifest LoadManifest(ProductManifestLocation manifestLocation, IFileInfo manifestFile)
        {
            return AvailableManifestBuilder.Build(manifestLocation, manifestFile);
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;
            _installedProduct ??= BuildProduct();
            if (_installedProduct is null)
                throw new InvalidOperationException("Created Product must not be null!");
            _isInitialized = true;
        }
    }
}