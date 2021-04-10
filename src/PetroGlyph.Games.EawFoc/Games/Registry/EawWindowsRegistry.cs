using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    public class EawWindowsRegistry : WindowsGameRegistry
    {
        private const string EawRegistryPath =
            @"SOFTWARE\LucasArts\Star Wars Empire at War";

        public EawWindowsRegistry(IFileSystem? fileSystem = null) : base(EawRegistryPath, fileSystem)
        {
        }
    }
}