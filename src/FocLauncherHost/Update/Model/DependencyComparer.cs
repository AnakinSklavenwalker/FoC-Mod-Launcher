using System;
using System.Collections.Generic;

namespace FocLauncherHost.Update.Model
{
    public class DependencyComparer : IEqualityComparer<Dependency>
    {
        public static readonly DependencyComparer Name = new DependencyComparer(CompareMode.Name);
        public static readonly DependencyComparer NameAndVersion = new DependencyComparer(CompareMode.NameAndVersion);
        public static readonly DependencyComparer Complete = new DependencyComparer(CompareMode.All);

        private readonly CompareMode _compareMode;
        private readonly StringComparer _ignoreCaseComparer = StringComparer.OrdinalIgnoreCase;
        private readonly StringComparer _comparer = StringComparer.InvariantCulture;

        public DependencyComparer(CompareMode compareMode)
        {
            _compareMode = compareMode;
        }

        public bool Equals(Dependency x, Dependency y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            if (!_ignoreCaseComparer.Equals(x.Name, y.Name))
                return false;
            if (_compareMode < CompareMode.NameAndVersion)
                return true;
            if (!_comparer.Equals(x.Version, y.Version))
                return false;
            if (_compareMode == CompareMode.NameAndVersion)
                return true;
            return Equals(x.Destination, y.Destination) && Equals(x.Origin, y.Destination) && Equals(x.Sha2, y.Sha2) && Equals(x.Size, y.Size);
        }

        public int GetHashCode(Dependency obj)
        {
            var num = 0;
            if (obj.Name != null)
                num ^= _ignoreCaseComparer.GetHashCode(obj.Name);
            if (_compareMode >= CompareMode.NameAndVersion && obj.Version != null &&
                Version.TryParse(obj.Version, out var version))
                num ^= version.GetHashCode();

            if (_compareMode != CompareMode.All)
                return num;

            if (obj.Size.HasValue)
                num ^= obj.Size.Value.GetHashCode();
            if (!string.IsNullOrEmpty(obj.Origin))
                num ^= _comparer.GetHashCode(obj.Origin);
            if (!string.IsNullOrEmpty(obj.Destination))
                num ^= _comparer.GetHashCode(obj.Destination);
            if (obj.Sha2 != null)
                num ^= obj.Sha2.GetHashCode();
            return num;
        }


        public enum CompareMode
        {
            Name = 1,
            NameAndVersion,
            All
        }
    }
}