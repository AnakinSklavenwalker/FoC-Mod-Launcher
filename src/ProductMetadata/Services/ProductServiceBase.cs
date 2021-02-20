using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductMetadata.Component;
using ProductMetadata.Manifest;
using ProductMetadata.Services.Detectors;
using Validation;

namespace ProductMetadata.Services
{
    public abstract class ProductServiceBase : IProductService
    {
        private readonly IServiceProvider _serviceProvider;
        private bool _isInitialized;
        private IInstalledProduct? _installedProduct;

        protected ILogger? Logger;
        protected readonly IFileSystem FileSystem;

        protected IAvailableManifestBuilder AvailableManifestBuilder { get; }
        
        protected IManifestFileResolver? ManifestFileResolver { get; private set; }

        protected IComponentDetectorFactory? ComponentDetectorFactory { get; private set; }

        protected ProductServiceBase(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
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
        
        private IEnumerable<IProductComponent> FindInstalledComponents(IManifest manifest, string installationPath)
        {
            var currentInstance = GetCurrentInstance();
            return manifest.Items.Select(component =>
            {
                var detector = ComponentDetectorFactory!.GetDetector(component.Type, _serviceProvider);
                return detector.Find(component, currentInstance);
            });
        }

        protected virtual IFileInfo GetAvailableManifestFile(ManifestLocation manifestLocation)
        {
            return ManifestFileResolver!.GetManifest(manifestLocation.ManifestUri);
        }
        
        protected virtual bool IsProductCompatible(IProductReference product)
        {
            return !ProductReferenceEqualityComparer.NameOnly.Equals(_installedProduct!, product);
        }
        
        protected virtual IManifest LoadManifest(ManifestLocation manifestLocation, IFileInfo manifestFile)
        {
            return AvailableManifestBuilder.Build(manifestLocation, manifestFile);
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;
            ManifestFileResolver = _serviceProvider.GetService<IManifestFileResolver>() ?? new LocalManifestFileResolver(_serviceProvider);
            ComponentDetectorFactory = _serviceProvider.GetService<IComponentDetectorFactory>() ?? new ComponentDetectorFactory();
            _installedProduct ??= BuildProduct();
            AddProductVariables(_installedProduct);
            if (_installedProduct is null)
                throw new InvalidOperationException("Created Product must not be null!");
            _isInitialized = true;
        }

        private void AddProductVariables(IInstalledProduct product)
        {
            var fs = _serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
            product.ProductVariables.Add(KnownProductVariablesKeys.InstallDir, product.InstallationPath);
            product.ProductVariables.Add(KnownProductVariablesKeys.InstallDrive, fs.Path.GetPathRoot(product.InstallationPath));
            AddAdditionalProductVariables(product);
        }

        protected virtual void AddAdditionalProductVariables(IInstalledProduct product)
        {
        }
    }
}