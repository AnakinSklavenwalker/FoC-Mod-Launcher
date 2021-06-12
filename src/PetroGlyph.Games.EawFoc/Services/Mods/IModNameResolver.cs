using System;
using System.Globalization;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services
{
    public interface IModNameResolver
    {
        event EventHandler<ModNameResolved> NameResolved;
        string ResolveName(IModReference modReference);
        string ResolveName(IModReference modReference, CultureInfo culture);
    }

    public class ModNameResolved : EventArgs
    {
        public string Name { get; }

        public CultureInfo Culture { get; }

        public ModNameResolved(string name) : this(name, CultureInfo.InvariantCulture)
        {
        }

        public ModNameResolved(string name, CultureInfo culture)
        {
            Name = name;
            Culture = culture;
        }
    }
}