using System;
using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class ModEqualityComparer : IEqualityComparer<IMod>
    {
        public static readonly ModEqualityComparer Default = new ModEqualityComparer(true, false);
        public static readonly ModEqualityComparer NameAndIdentifier = new ModEqualityComparer(true, true);
        //public static readonly ModEqualityComparer NamEqualityComparer = new ModEqualityComparer(true);

        private readonly bool _default;
        private readonly bool _useName;

        private readonly StringComparer _ignoreCaseComparer = StringComparer.OrdinalIgnoreCase;

        public ModEqualityComparer(bool useIdentifier, bool useName)
        {
            _default = useIdentifier;
            _useName = useName;
        }

        public bool Equals(IMod x, IMod y)
        {
            if (x is null || y is null)
                return false;
            if (x == y)
                return true;

            if (_useName)
                if (!_ignoreCaseComparer.Equals(((IModIdentity) x).Name, ((IModIdentity) y).Name))
                    return false;

            if (_default)
                return x.Equals(y);
            throw new NotImplementedException();
        }

        public int GetHashCode(IMod obj)
        {
            var num = 0;
            var name = ((IModIdentity) obj).Name;
            if (name != null)
                num ^= _ignoreCaseComparer.GetHashCode(name);
            if (_default)
                num ^= obj.GetHashCode();
            return num;
        }
    }
}