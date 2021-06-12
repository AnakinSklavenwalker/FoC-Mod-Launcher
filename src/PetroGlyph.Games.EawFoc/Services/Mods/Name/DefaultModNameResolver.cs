using System;
using System.Collections.Generic;
using System.Globalization;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Name
{
    public sealed class DefaultModNameResolver : IModNameResolver
    {
        private readonly IList<IModNameResolver> _sortedResolvers;
        public event EventHandler<ModNameResolved>? NameResolved;

        public DefaultModNameResolver(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _sortedResolvers = new List<IModNameResolver>
            {
                new OnlineWorkshopNameResolver(serviceProvider),
                new DirectoryModNameResolver(serviceProvider)
            };
        }
        
        public string ResolveName(IModReference modReference)
        {
            return ResolveName(modReference, CultureInfo.InvariantCulture);
        }

        public string ResolveName(IModReference modReference, CultureInfo culture)
        {
            foreach (var nameResolver in _sortedResolvers)
            {
                nameResolver.NameResolved += OnNameResolved;
                try
                {
                    return nameResolver.ResolveName(modReference, culture);
                }
                finally
                {
                    nameResolver.NameResolved -= OnNameResolved;
                }
            }
            throw new ModException($"Unable to resolve the mod's name {modReference}");
        }

        private void OnNameResolved(object? sender, ModNameResolved e)
        {
            NameResolved?.Invoke(this, e);
        }
    }
}