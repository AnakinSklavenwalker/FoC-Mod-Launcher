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

        IAvailableProductCatalog? GetAvailableProductCatalog(IUpdateRequest product);
    }


    public interface IInstalledProduct : IProductReference
    {
        string InstallationPath { get; }

        string Author { get; }

        DateTime? UpdateDate { get; }

        DateTime? InstallDate { get; }
    }

    public class InstalledProduct : IInstalledProduct
    {
        public string Name { get; init; }
        public Version? Version { get; init; }
        public ProductReleaseType ReleaseType { get; init; }
        public string InstallationPath { get; init; }
        public string Author { get; init; }
        public DateTime? UpdateDate { get; init; }
        public DateTime? InstallDate { get; init; }
    }
    
    public interface IUpdateRequest
    {
        string UpdateManifestPath { get; }
        
        UpdateRequestAction RequestedAction { get; }

        IProductReference Product { get; }
    }

    public sealed record UpdateRequest : IUpdateRequest
    {
        public string UpdateManifestPath { get; init; }
        public UpdateRequestAction RequestedAction { get; init; }
        public IProductReference Product { get; init; }
    }

    [Flags]
    public enum UpdateRequestAction
    {
        Update = 1,
        Repair = 2
    }


    public class ProductReferenceEqualityComparer : IEqualityComparer<IProductReference>
    {
        private readonly bool _compareVersion;
        private readonly bool _compareRelease;
        public static ProductReferenceEqualityComparer Default = new(true, true);
        public static ProductReferenceEqualityComparer VersionAware = new(true, false);
        public static ProductReferenceEqualityComparer ReleaseAware = new(true, true);


        private ProductReferenceEqualityComparer(bool compareVersion, bool compareRelease)
        {
            _compareVersion = compareVersion;
            _compareRelease = compareRelease;
        }

        public bool Equals(IProductReference? x, IProductReference? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            if (!x.Name.Equals(y.Name))
                return false;

            if (_compareRelease)
            {
                if (!x.ReleaseType.Equals(y.ReleaseType))
                    return false;
            }

            if (_compareVersion)
                return x.Version != null ? x.Version.Equals(y.Version) : y.Version == null;

            return true;
        }

        public int GetHashCode(IProductReference obj)
        {
#if NET
            if (_compareRelease && _compareVersion)
                return HashCode.Combine(obj.Name, obj.ReleaseType, obj.Version);
            if (_compareRelease)
                return HashCode.Combine(obj.Name, obj.ReleaseType);
            if (_compareVersion)
                return HashCode.Combine(obj.Name, obj.Version);
            return obj.Name.GetHashCode();
#else
           unchecked
            {
                var hashCode = obj.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.Version != null ? obj.Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) obj.ReleaseType;
                return hashCode;
            }
#endif
        }
    }


    public interface IProductReference
    {
        string Name { get; }

        Version? Version { get; }
        
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