using System;
using System.IO;
using FocLauncher;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Product.Component;
using TaskBasedUpdater.New.Product.Manifest;

namespace FocLauncherHost.Product
{
    internal sealed class LauncherProductService : ProductServiceBase
    {
        public LauncherProductService(
            IProductComponentBuilder componentBuilder, 
            IAvailableManifestBuilder updateManifestBuilder, 
            IServiceProvider serviceProvider) : base(componentBuilder, updateManifestBuilder, serviceProvider)
        {
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
}