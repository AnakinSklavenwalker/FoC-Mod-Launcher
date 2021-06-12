using System;
using System.Globalization;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Name
{
    public abstract class ModNameResolverBase : IModNameResolver
    {
        protected readonly IServiceProvider ServiceProvider;
        public event EventHandler<ModNameResolved>? NameResolved;

        protected ModNameResolverBase(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            ServiceProvider = serviceProvider;
        }

        public string ResolveName(IModReference modReference)
        {
            return ResolveName(modReference, CultureInfo.InvariantCulture);
        }

        public string ResolveName(IModReference modReference, CultureInfo culture)
        {
            Requires.NotNull(modReference, nameof(modReference));
            Requires.NotNull(culture, nameof(culture));

            string modName;
            try
            {
                modName = ResolveCore(modReference, culture);
            }
            catch (Exception e)
            {
                throw new ModException($"Unable to resolve the mod's name {modReference}: {this}", e);
            }
            if (string.IsNullOrEmpty(modName))
                throw new ModException($"Resolved name for {modReference} was null or empty: {this}");

            OnNameResolved(new ModNameResolved(modName, culture));
            return modName;
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        protected abstract string ResolveCore(IModReference modReference, CultureInfo culture);

        protected virtual void OnNameResolved(ModNameResolved e)
        {
            NameResolved?.Invoke(this, e);
        }
    }
}