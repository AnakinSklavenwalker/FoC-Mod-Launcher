using System;
using Validation;

namespace TaskBasedUpdater.New.Product
{
    public class InstalledProduct : ProductReference, IInstalledProduct
    {
        public string InstallationPath { get; }
        public string? Author { get; init; }
        public DateTime? UpdateDate { get; init; }
        public DateTime? InstallDate { get; init; }

        public InstalledProduct(string name, string installationPath) : base(name)
        {
            Requires.NotNullOrEmpty(installationPath, nameof(installationPath));
            InstallationPath = installationPath;
        }

        public override string ToString()
        {
            return $"Product '{Name}:{Version}:{ReleaseType}' at {InstallationPath}";
        }
    }
}