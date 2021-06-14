using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public sealed class VirtualMod : ModBase, IVirtualMod
    {
        public override string Identifier { get; }

        public VirtualMod(IGame game, IModinfo modInfoData, IServiceProvider serviceProvider) 
            : base(game, ModType.Virtual, modInfoData, serviceProvider)
        {
            if (modInfoData.Dependencies is null || !modInfoData.Dependencies.Any())
                throw new ModException("Virtual mods must be initialized with pre-defined dependencies");

            foreach (var dependency in modInfoData.Dependencies)
            {
                var mod = game.Mods.FirstOrDefault(m => m.Equals(dependency));
                if (mod is null)
                    throw new ModException(
                        $"Could not find dependency '{dependency.Identifier}-{dependency.Type}' in {game}");
                DependenciesInternal.Add(mod);
            }

            AssureVirtualModIsValid();

            Identifier = CalculateIdentifier();
        }

        public VirtualMod(string name, IGame game, IList<IMod> dependencies, IServiceProvider serviceProvider) 
            : base(game, ModType.Virtual, name, serviceProvider)
        {
            Requires.NotNullOrEmpty(dependencies, nameof(dependencies));

            foreach (var dependency in dependencies)
            {
                if (!dependency.Game.Equals(game))
                    throw new ModException($"Game of mod {dependency} does not match this mod's game.");
                DependenciesInternal.Add(dependency);
            }

            AssureVirtualModIsValid();

            Identifier = CalculateIdentifier();
        }
        
        public override string ToString()
        {
            return Name + "-" + Identifier;
        }

        public override bool Equals(IMod? other)
        {
            return other is VirtualMod && base.Equals(other);
        }

        public override bool Equals(IModIdentity? other)
        {
            return other is VirtualMod && base.Equals(other);
        }

        public override bool Equals(IModReference? other)
        {
            return other is VirtualMod && base.Equals(other);
        }

        protected override void OnResolvingModinfo(ResolvingModinfoEventArgs e)
        {
            throw new InvalidOperationException("Virtual mods cannot lazy load modinfo data");
        }

        protected override void OnDependenciesChanged(ModDependenciesChangedEventArgs e)
        {
            throw new InvalidOperationException("Virtual mods cannot lazy load modinfo data");
        }

        private void AssureVirtualModIsValid()
        {
            if (new ModDependencyTraverser(this).HasDependencyCycles())
                throw new ModException("Dependency Cycle detected");

            if (!Dependencies.Any(m => m is IPhysicalPlayableObject))
                throw new ModException("Virtual mods need at least one physical dependency.");
        } 
        
        private string CalculateIdentifier()
        {
            var id = Dependencies.Aggregate(Name, (current, dependency) => current + dependency.GetHashCode());
            return id.GetHashCode().ToString();
        }
    }
}