using System;
using ProductMetadata.Manifest;
using Validation;

namespace ProductMetadata
{
    public sealed class InstalledProduct : IInstalledProduct
    {
        private readonly IProductReference _reference;

        public string Name => _reference.Name;
        public Version? Version => _reference.Version;
        public string? Branch => _reference.Branch;

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
            _reference = reference;
            InstallationPath = installationPath;
            CurrentManifest = manifest;
            ProductVariables = new VariableCollection();
        }

        public override string ToString()
        {
            return $"Product '{_reference.Name} v{_reference.Version} Branch:{_reference.Branch?? string.Empty}' " +
                   $"at {InstallationPath}";
        }
    }
}