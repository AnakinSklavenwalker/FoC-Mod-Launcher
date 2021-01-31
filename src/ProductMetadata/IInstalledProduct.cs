using System;
using ProductMetadata.Manifest;

namespace ProductMetadata
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