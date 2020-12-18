using System;
using Validation;

namespace TaskBasedUpdater.New.Product
{
    public sealed class InstalledProduct : ProductReference, IInstalledProduct
    {
        public string InstallationPath { get; }
        public IInstalledProductManifest ProductManifest { get; }
        public string? Author { get; init; }
        public DateTime? UpdateDate { get; init; }
        public DateTime? InstallDate { get; init; }

        public InstalledProduct(string name, string installationPath, IInstalledProductManifest manifest) : base(name)
        {
            Requires.NotNullOrEmpty(installationPath, nameof(installationPath));
            Requires.NotNull(manifest, nameof(manifest));
            InstallationPath = installationPath;
            ProductManifest = manifest;
        }

        public override string ToString()
        {
            return $"Product '{Name}:{Version}:{ReleaseType}' at {InstallationPath}";
        }
    }
}