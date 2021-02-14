using System;
using ProductMetadata.Manifest;

namespace ProductMetadata
{
    public interface IInstalledProduct
    {
        IProductReference ProductReference { get; }
        
        string InstallationPath { get; }

        IManifest CurrentManifest { get; }
        
        string? Author { get; }

        DateTime? UpdateDate { get; }

        DateTime? InstallDate { get; }

        ProductReleaseType ReleaseType { get; }

        VariableCollection ProductVariables { get; }
    }
}