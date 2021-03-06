using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public sealed class VirtualMod : ModBase
    {
        public override string Identifier => throw new NotImplementedException();

        public VirtualMod(IGame game, IModinfo? modInfoData) : base(game, ModType.Virtual, modInfoData)
        {
            // TODO: modinfo dependencies must not be null or empty!
            throw new NotImplementedException();
        }

        public VirtualMod(string name, IGame game, IList<IMod> dependencies) : base(name, game, ModType.Virtual)
        {
            // TODO: dependencies must not be null or empty!
            throw new NotImplementedException();
        }

        public override bool Equals(IMod other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(IModIdentity other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(IModReference other)
        {
            throw new NotImplementedException();
        }

        public override string ToArgs(bool includeDependencies)
        {
            if (Virtual && !includeDependencies)
                throw new InvalidOperationException("A virtual mod cannot yield arguments. " +
                                                    $"Use parameter '{nameof(includeDependencies)}' instead!");

            return string.Empty;
        }

        protected override bool ResolveDependenciesCore()
        {
            throw new NotImplementedException();
        }
    }
}