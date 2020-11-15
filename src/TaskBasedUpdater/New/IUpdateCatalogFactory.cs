using System;
using System.Collections.Generic;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.New
{

    public interface ICatalog
    {
        IEnumerable<IUpdateItem> Items { get; }
    }

    public interface IInstalledProductCatalog : ICatalog
    {
        IInstalledProduct Product { get; }
    }

    public interface IAvailableProductCatalog : ICatalog
    {
        IProductReference Product { get; }
    }



    public interface IUpdateCatalog : ICatalog
    {
        IProductReference Product { get; }

        IEnumerable<IUpdateItem> ItemsToInstall { get; }
        IEnumerable<IUpdateItem> ItemsToKeep { get; }
        IEnumerable<IUpdateItem> ItemsToDelete { get; }
    }

    public interface IUpdateCatalogBuilder
    {
        IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductCatalog availableCatalog);
    }



    public interface IProductProviderService
    {
        IInstalledProductCatalog GetInstalledProductCatalog(IInstalledProduct product);

        IAvailableProductCatalog? GetAvailableProductCatalog(IProductReference product);
    }


    public interface IInstalledProduct : IProductReference
    {
        string InstallationPath { get; }

        string Author { get; }

        DateTime UpdateData { get; }

        DateTime InstallDate { get; }
    }


    public interface IProductReference
    {
        string Name { get; }

        Version Version { get; }
        
        ProductReleaseType ReleaseType { get; }
    }

    public enum ProductReleaseType
    {
        Stable,
        PreAlpha,
        Beta,
        ReleaseCandidate,
        Rtm
    }
}