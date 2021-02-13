using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
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

        protected IAvailableManifestBuilder AvailableManifestBuilder { get; }
        
        protected IManifestFileResolver ManifestFileResolver { get; }

        protected IComponentDetectorFactory ComponentDetectorFactory{ get; }

        protected ProductServiceBase(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            ManifestFileResolver = serviceProvider.GetRequiredService<IManifestFileResolver>();
            AvailableManifestBuilder = serviceProvider.GetRequiredService<IAvailableManifestBuilder>();

            FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
            Logger = serviceProvider.GetService<ILogger>();
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

        public abstract IProductReference CreateProductReference(Version? newVersion, string? branch);

        public IInstalledProductCatalog GetInstalledProductCatalog()
        {
            Initialize();
            var manifest = _installedProduct!.CurrentManifest;
            var installPath = _installedProduct.InstallationPath;
            return new InstalledProductCatalog(_installedProduct!, FindInstalledComponents(manifest, installPath));
        }

        public IManifest GetAvailableProductManifest(ManifestLocation manifestLocation)
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
                    var manifest = LoadManifest(manifestLocation, manifestFile);
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

        protected virtual IEnumerable<IProductComponent> FindInstalledComponents(IManifest manifest, string installationPath)
        {
            var currentInstance = GetCurrentInstance();
            return manifest.Items.Select(component =>
            {
                var detector = ComponentDetectorFactory.GetDetector(component.Type);
                return detector.Find(component, currentInstance);
            });
        }

        protected virtual IFileInfo GetAvailableManifestFile(ManifestLocation manifestLocation)
        {
            return ManifestFileResolver.GetManifest(manifestLocation.ManifestUri);
        }
        
        protected virtual bool IsProductCompatible(IProductReference product)
        {
            return !ProductReferenceEqualityComparer.NameOnly.Equals(_installedProduct!.ProductReference, product);
        }
        
        protected virtual IManifest LoadManifest(ManifestLocation manifestLocation, IFileInfo manifestFile)
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