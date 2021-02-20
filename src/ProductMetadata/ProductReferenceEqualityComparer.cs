using System.Collections.Generic;
#if NET || NETSTANDARD2_1
using System;
#endif

namespace ProductMetadata
{
    public class ProductReferenceEqualityComparer : IEqualityComparer<IProductReference>
    {
        private readonly bool _compareVersion;
        private readonly bool _compareBranch;
        public static ProductReferenceEqualityComparer Default = new(true, true);
        public static ProductReferenceEqualityComparer VersionAware = new(true, false);
        public static ProductReferenceEqualityComparer BranchAware = new(true, true);
        public static ProductReferenceEqualityComparer NameOnly = new(false, false);


        private ProductReferenceEqualityComparer(bool compareVersion, bool compareBranch)
        {
            _compareVersion = compareVersion;
            _compareBranch = compareBranch;
        }

        public bool Equals(IProductReference? x, IProductReference? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            if (!x.Name.Equals(y.Name))
                return false;

            if (_compareBranch)
            {
                if (!Equals(x.Branch, y.Branch))
                    return false;
            }

            if (_compareVersion)
                return x.Version != null ? x.Version.Equals(y.Version) : y.Version == null;

            return true;
        }

        public int GetHashCode(IProductReference obj)
        {
#if NET || NETSTANDARD2_1
            if (_compareBranch && _compareVersion)
                return HashCode.Combine(obj.Name, obj.Branch, obj.Version);
            if (_compareBranch)
                return HashCode.Combine(obj.Name, obj.Branch);
            if (_compareVersion)
                return HashCode.Combine(obj.Name, obj.Version);
            return obj.Name.GetHashCode();
#else
            unchecked
            {
                var hashCode = obj.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.Version != null ? obj.Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Branch != null ? obj.Branch.GetHashCode() : 0);
                return hashCode;
            }
#endif
        }

    }
}