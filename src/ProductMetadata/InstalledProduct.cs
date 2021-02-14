using System;
using ProductMetadata.Manifest;
using Validation;

namespace ProductMetadata
{
    public sealed class InstalledProduct : IInstalledProduct
    {
        public IProductReference ProductReference { get; }
        public string InstallationPath { get; }
        public IManifest CurrentManifest { get; }
        public string? Author { get; init; }
        public DateTime? UpdateDate { get; init; }
        public DateTime? InstallDate { get; init; }
        public ProductReleaseType ReleaseType { get; init; }
        public VariableCollection ProductVariables { get; }

        public InstalledProduct(IProductReference reference, IManifest manifest, string installationPath)
        {
            Requires.NotNull(reference, nameof(reference));
            Requires.NotNullOrEmpty(installationPath, nameof(installationPath));
            Requires.NotNull(manifest, nameof(manifest));
            ProductReference = reference;
            InstallationPath = installationPath;
            CurrentManifest = manifest;
            ProductVariables = new VariableCollection();
        }

        public override string ToString()
        {
            return $"Product '{ProductReference}' at {InstallationPath}";
        }
    }
}