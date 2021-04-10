using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    public class FocWindowsRegistry : WindowsGameRegistry
    {
        private const string FocRegistryPath =
            @"SOFTWARE\LucasArts\Star Wars Empire at War Forces of Corruption";

        public FocWindowsRegistry(IFileSystem? fileSystem = null) : base(FocRegistryPath, fileSystem)
        {
        }
    }
}