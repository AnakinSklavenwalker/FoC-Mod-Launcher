using System;

namespace TaskBasedUpdater.New.Product
{
    public interface IInstalledProduct : IProductReference
    {
        string InstallationPath { get; }

        string? Author { get; }

        DateTime? UpdateDate { get; }

        DateTime? InstallDate { get; }
    }
}