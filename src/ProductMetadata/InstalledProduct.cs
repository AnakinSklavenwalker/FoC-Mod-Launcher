using System;
using ProductMetadata.Manifest;
using Validation;

namespace ProductMetadata
{
    public sealed class InstalledProduct : IInstalledProduct
    {
        public IProductReference ProductReference { get; }
        public string InstallationPath { get; }
        public IInstalledProductManifest ProductManifest { get; }
        public string? Author { get; init; }
        public DateTime? UpdateDate { get; init; }
        public DateTime? InstallDate { get; init; }

        public InstalledProduct(IProductReference reference, IInstalledProductManifest manifest, string installationPath)
        {
            Requires.NotNull(reference, nameof(reference));
            Requires.NotNullOrEmpty(installationPath, nameof(installationPath));
            Requires.NotNull(manifest, nameof(manifest));
            ProductReference = reference;
            InstallationPath = installationPath;
            ProductManifest = manifest;
        }

        public override string ToString()
        {
            return $"Product '{ProductReference}' at {InstallationPath}";
        }
    }
}