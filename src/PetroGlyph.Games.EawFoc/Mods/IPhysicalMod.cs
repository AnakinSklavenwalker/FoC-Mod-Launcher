namespace PetroGlyph.Games.EawFoc.Mods
{
    /// <summary>
    /// Represents an <see cref="IMod"/> which is bound to a real location on the file system.
    /// Implements <see cref="IPhysicalPlayableObject"/>
    /// </summary>
    public interface IPhysicalMod : IMod, IPhysicalPlayableObject
    {
    }
}