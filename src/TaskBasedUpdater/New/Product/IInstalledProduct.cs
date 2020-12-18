using System;

namespace TaskBasedUpdater.New.Product
{
    public interface IInstalledProduct : IProductReference
    {
        string InstallationPath { get; }

        IInstalledProductManifest ProductManifest { get; }

        string? Author { get; }

        DateTime? UpdateDate { get; }

        DateTime? InstallDate { get; }
    }

    public interface IInstalledProductManifest : ICatalog
    {
    }
}