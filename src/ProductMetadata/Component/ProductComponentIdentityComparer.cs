using System;
using System.Collections.Generic;

namespace ProductMetadata.Component
{
    public class ProductComponentIdentityComparer : IEqualityComparer<IProductComponentIdentity>
    {
        public static readonly ProductComponentIdentityComparer Default = new();
        public static readonly ProductComponentIdentityComparer VersionIndependent = new(true);
        public static readonly ProductComponentIdentityComparer VersionAndBranchIndependent = new(true, excludeBranch: true);
        private readonly StringComparison _comparisonType;
        private readonly bool _excludeVersion;
        private readonly bool _excludeBranch;
        private readonly StringComparer _comparer;

        public ProductComponentIdentityComparer(
            bool excludeVersion = false,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase,
            bool excludeBranch = false)
        {
            _excludeVersion = excludeVersion;
            _comparisonType = comparisonType;
            _excludeBranch = excludeBranch;
            _comparer = comparisonType switch
            {
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.Ordinal => StringComparer.Ordinal,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                _ => throw new ArgumentException("The comparison type is not supported", nameof(comparisonType))
            };
        }

        public bool Equals(IProductComponentIdentity? x, IProductComponentIdentity? y)
        {
            if (x == y)
                return true;
            if (x is null || y is null)
                return false;
            if (!x.Id.Equals(y.Id, _comparisonType))
                return false;
            if (!_excludeBranch)
            {
                if (!string.Equals(x.Branch, y.Branch, _comparisonType))
                    return false;
            }
            if (!_excludeVersion)
            {
                if (x.Version != null)
                    return x.Version.Equals(y.Version);
                return y.Version == null;
            }

            return true;
        }

        public int GetHashCode(IProductComponentIdentity? obj)
        {
            if (obj == null)
                return 0;
            var num = 0;
            if (obj.Id != null)
                num ^= _comparer.GetHashCode(obj.Id);
            if (!_excludeBranch && obj.Branch != null)
                num ^= _comparer.GetHashCode(obj.Branch);
            if (!_excludeVersion && obj.Version != null)
                num ^= obj.Version.GetHashCode();
            return num;
        }
    }
}