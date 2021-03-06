using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class ModEqualityComparer : IEqualityComparer<IMod>, IEqualityComparer<IModIdentity>, IEqualityComparer<IModReference>
    {
        public static readonly ModEqualityComparer Default = new(true, false);
        public static readonly ModEqualityComparer NameAndIdentifier = new(true, true);
        public static readonly ModEqualityComparer NamEqualityComparer = new ModEqualityComparer(false, true);

        private readonly bool _default;
        private readonly bool _useName;

        private readonly StringComparer _ignoreCaseComparer = StringComparer.OrdinalIgnoreCase;

        public ModEqualityComparer(bool useIdentifier, bool useName)
        {
            _default = useIdentifier;
            _useName = useName;
        }

        public bool Equals(IMod? x, IMod? y)
        {
            if (x is null || y is null)
                return false;
            if (x == y)
                return true;

            if (_useName)
                if (!_ignoreCaseComparer.Equals(x.Name, y.Name))
                    return false;

            if (_default)
                return x.Equals(y);
            throw new NotImplementedException();
        }

        public int GetHashCode(IMod obj)
        {
            var num = 0;
            var name = obj.Name;
            if (name != null)
                num ^= _ignoreCaseComparer.GetHashCode(name);
            if (_default)
                num ^= obj.GetHashCode();
            return num;
        }

        public bool Equals(IModIdentity x, IModIdentity y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(IModIdentity obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IModReference? x, IModReference? y)
        {
            if (x is null || y is null)
                return false;
            return ReferenceEquals(x, y) || new ModReference(x).Equals(new ModReference(y));
        }

        public int GetHashCode(IModReference obj)
        {
            return new ModReference(obj).GetHashCode();
        }
    }
}