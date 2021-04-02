using System.IO;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc
{
    public interface IPhysicalPlayableObject : IPlayableObject
    {
        /// <summary>
        /// Returns a <see cref="DirectoryInfo"/> of the root directory.
        /// </summary>
        IDirectoryInfo Directory { get; }
    }
}