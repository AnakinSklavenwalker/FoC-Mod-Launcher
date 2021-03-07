using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class ModEqualityComparer : IEqualityComparer<IMod>, IEqualityComparer<IModIdentity>, IEqualityComparer<IModReference>
    {
        public static readonly ModEqualityComparer Default = new(true, true);
        public static readonly ModEqualityComparer ExcludeDependencies = new(false, true);
        public static readonly ModEqualityComparer ExcludeGame = new(true, false);

        private readonly bool _includeDependencies;
        private readonly bool _includeGameReference;

        private readonly StringComparer _ignoreCaseComparer = StringComparer.OrdinalIgnoreCase;

        public ModEqualityComparer(bool includeDependencies, bool includeGameReference)
        {
            _includeDependencies = includeDependencies;
            _includeGameReference = includeGameReference;
        }

        public bool Equals(IMod? x, IMod? y)
        {
            if (x is null || y is null)
                return false;
            if (x == y)
                return true;

            if (!x.Identifier.Equals(y.Identifier, StringComparison.Ordinal))
                return false;

            if (_includeGameReference)
            {
                if (!x.Game.Equals(y.Game))
                    return false;
            }

            if (_includeDependencies)
            {
                if (!x.Dependencies.Count.Equals(y.Dependencies.Count))
                    return false;

                if (!x.Dependencies.SequenceEqual(y.Dependencies))
                    return false;
            }

            return true;
        }

        public int GetHashCode(IMod obj)
        {
            var num = 0;
            var id = obj.Identifier;
            num ^= _ignoreCaseComparer.GetHashCode(id);
            if (_includeGameReference)
                num ^= obj.Game.GetHashCode();
            if (_includeDependencies)
                num ^= obj.Dependencies.GetHashCode();
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