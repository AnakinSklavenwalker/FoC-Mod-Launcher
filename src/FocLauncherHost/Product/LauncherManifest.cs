using System.Collections.Generic;
using FocLauncher;
using Microsoft;
using ProductMetadata;
using ProductMetadata.Component;
using ProductMetadata.Manifest;

namespace FocLauncherHost.Product
{
    internal class LauncherManifest : IInstalledProductManifest
    {
        public LauncherManifest(IProductReference product)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }

        public IEnumerable<ProductComponent> Items
        {
            get
            {
                yield return new ProductComponent(LauncherConstants.LauncherFileName, "");

                yield return new ProductComponent(LauncherConstants.UpdaterFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");

                yield return new ProductComponent(LauncherConstants.LauncherDllFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");

                yield return new ProductComponent(LauncherConstants.LauncherThemeFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");

                yield return new ProductComponent(LauncherConstants.LauncherThreadingFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");
            }
        }

        public IProductReference Product { get; }
    }
}