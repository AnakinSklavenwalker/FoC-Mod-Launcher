using System;
using System.IO.Abstractions;
using System.Threading;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public class DirectoryGameDetector : GameDetector
    {
        private readonly IDirectoryInfo _directory;

        private static DirectoryGameDetector? _currentDirectoryDetector;

        public DirectoryGameDetector(IDirectoryInfo directory, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Requires.NotNull(directory, nameof(directory));
            _directory = directory;
        }

        public static DirectoryGameDetector CurrentDirectoryGameDetector(IServiceProvider serviceProvider)
        {
            return LazyInitializer.EnsureInitialized(ref _currentDirectoryDetector, () =>
            {
                var fs = new FileSystem();
                var currentDir = fs.DirectoryInfo.FromDirectoryName(fs.Directory.GetCurrentDirectory());
                return new DirectoryGameDetector(currentDir, serviceProvider);
            })!;
        }

        protected override GameLocationData FindGameLocation(GameType type)
        {
            return GameExeExists(_directory, type)
                ? new GameLocationData {Location = _directory}
                : new GameLocationData();
        }
    }
}