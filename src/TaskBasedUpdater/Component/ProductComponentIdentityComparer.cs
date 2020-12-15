using System;
using System.Collections.Generic;

namespace TaskBasedUpdater.Component
{
    public class ProductComponentIdentityComparer : IEqualityComparer<ProductComponent>
    {
        public static readonly ProductComponentIdentityComparer Default = new();
        public static readonly ProductComponentIdentityComparer VersionIndependent = new(true);

        private readonly bool _excludeVersion;
        private readonly StringComparison _comparisonType;
        private readonly StringComparer _comparer;

        public ProductComponentIdentityComparer(bool excludeVersion = false, 
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            _excludeVersion = excludeVersion;
            _comparisonType = comparisonType;

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    _comparer = StringComparer.CurrentCulture;
                    break;
                case StringComparison.CurrentCultureIgnoreCase:
                    _comparer = StringComparer.CurrentCultureIgnoreCase;
                    break;
                case StringComparison.Ordinal:
                    _comparer = StringComparer.Ordinal;
                    break;
                case StringComparison.OrdinalIgnoreCase:
                    _comparer = StringComparer.OrdinalIgnoreCase;
                    break;
                default:
                    throw new ArgumentException("The comparison type is not supported", nameof(comparisonType));
            }
        }

        public bool Equals(ProductComponent? x, ProductComponent? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            var flag = x.Name.Equals(y.Name) && x.Destination.Equals(y.Destination);
            if (!flag)
                return false;

            if (_excludeVersion)
                return true;

            if (x.CurrentVersion == y.CurrentVersion)
                return true;
            return x.CurrentVersion is not null && x.CurrentVersion.Equals(y.CurrentVersion);
        }

        public int GetHashCode(ProductComponent? obj)
        {
            if (obj is null)
                return 0;
            var num = 0;

            num ^= _comparer.GetHashCode(obj.Name);
            num ^= _comparer.GetHashCode(obj.Destination);

            if (!_excludeVersion && obj.CurrentVersion != null)
                num ^= obj.CurrentVersion.GetHashCode();
            return num;
        }
    }
}