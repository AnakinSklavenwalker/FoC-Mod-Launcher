using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public sealed class VirtualMod : ModBase
    {
        public override string Identifier => throw new NotImplementedException();

        public VirtualMod(IGame game, IModinfo modInfoData) : base(game, ModType.Virtual, modInfoData)
        {
            // TODO: modinfo dependencies must not be null or empty!
            throw new NotImplementedException();
        }

        public VirtualMod(string name, IGame game, IList<IMod> dependencies) : base(game, ModType.Virtual, name)
        {
            Requires.NotNullOrEmpty(dependencies, nameof(dependencies));
            // TODO: dependencies must not be null or empty!
            throw new NotImplementedException();
        }

        public override bool Equals(IMod other)
        {
            return other is VirtualMod && base.Equals(other);
        }

        public override bool Equals(IModIdentity other)
        {
            return other is VirtualMod && base.Equals(other);
        }

        public override bool Equals(IModReference other)
        {
            return other is VirtualMod && base.Equals(other);
        }

        protected override void OnResolvingModinfo(ResolvingModinfoEventArgs e)
        {
            throw new InvalidOperationException("Virtual mods cannot lazy load modinfo data");
        }
    }
}