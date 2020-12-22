using System;
using TaskBasedUpdater.New.Product.Manifest;

namespace TaskBasedUpdater.New.Product
{
    public interface IInstalledProduct
    {
        IProductReference ProductReference { get; }
        
        string InstallationPath { get; }

        IInstalledProductManifest ProductManifest { get; }
        
        string? Author { get; }

        DateTime? UpdateDate { get; }

        DateTime? InstallDate { get; }
    }
}