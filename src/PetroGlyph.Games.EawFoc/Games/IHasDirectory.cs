using System.IO;

namespace PetroGlyph.Games.EawFoc.Games
{
    public interface IHasDirectory
    {
        /// <summary>
        /// Returns a <see cref="DirectoryInfo"/> of the root directory.
        /// </summary>
        DirectoryInfo Directory { get; }
    }
}