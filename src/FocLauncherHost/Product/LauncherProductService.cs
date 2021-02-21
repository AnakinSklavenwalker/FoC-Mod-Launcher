using System;
using System.IO;
using System.IO.Abstractions;
using FocLauncher;
using Microsoft.Extensions.Logging;
using ProductMetadata;
using ProductMetadata.Manifest;
using ProductMetadata.Services;

namespace FocLauncherHost.Product
{
    //internal sealed class LauncherProductService : ProductServiceBase
    //{
    //    public LauncherProductService(
    //        IProductComponentBuilder componentBuilder, 
    //        IAvailableManifestBuilder updateManifestBuilder, 
    //        IFileSystem fileSystem, 
    //        ILogger? logger = null) 
    //        : base(componentBuilder, new LocalManifestFileResolver(fileSystem, logger), updateManifestBuilder, 
    //            new ComponentFileFactory(new ComponentFullDestinationResolver(fileSystem), fileSystem, Directory.GetCurrentDirectory()), fileSystem, logger)
    //    {
    //    }

    //    public override IProductReference CreateProductReference(Version? newVersion,
    //        ProductReleaseType newReleaseType)
    //    {
    //        return new ProductReference(LauncherConstants.ProductName)
    //        {
    //            Version = newVersion,
    //            ReleaseType = newReleaseType
    //        };
    //    }

    //    protected override IInstalledProduct BuildProduct()
    //    {
    //        var productRef = CreateProductReference(null, ProductReleaseType.Stable);
    //        var path = GetInstallationPath();
    //        var manifest = GetManifest(productRef);
    //        return new InstalledProduct(productRef, manifest, path);
    //    }

    //    protected override bool IsProductCompatible(IProductReference product)
    //    {
    //        return ProductReferenceEqualityComparer.NameOnly.Equals(GetCurrentInstance().ProductReference, product);
    //    }


    //    private static IInstalledProductManifest GetManifest(IProductReference productReference)
    //    {
    //        return new LauncherManifest(productReference);
    //    }

    //    private static string GetInstallationPath()
    //    {
    //        return Directory.GetCurrentDirectory();
    //    }
    //}
}