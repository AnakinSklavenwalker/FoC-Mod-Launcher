using System;
using System.Collections.Generic;

namespace TaskBasedUpdater.New.Product
{
    public class ProductReferenceEqualityComparer : IEqualityComparer<IProductReference>
    {
        private readonly bool _compareVersion;
        private readonly bool _compareRelease;
        public static ProductReferenceEqualityComparer Default = new(true, true);
        public static ProductReferenceEqualityComparer VersionAware = new(true, false);
        public static ProductReferenceEqualityComparer ReleaseAware = new(true, true);
        public static ProductReferenceEqualityComparer NameOnly = new(false, false);


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
}