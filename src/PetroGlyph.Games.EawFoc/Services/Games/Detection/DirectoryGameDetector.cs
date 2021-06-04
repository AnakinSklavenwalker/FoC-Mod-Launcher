using System;
using System.IO.Abstractions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Detects whether a given directory contains a Petroglyph Star Wars Game
    /// </summary>
    public sealed class DirectoryGameDetector : GameDetector
    {
        private readonly IDirectoryInfo _directory;

        private static DirectoryGameDetector? _currentDirectoryDetector;

        public DirectoryGameDetector(IDirectoryInfo directory, IServiceProvider serviceProvider) : base(serviceProvider, false)
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

        private protected override GameLocationData FindGameLocation(GameDetectorOptions options)
        {
            Logger?.LogDebug($"Searching for game {options.Type} at directory: {_directory}");
            // TODO: Maybe we should look into well-known sub directories

            // We directly return die location data, because we relay on the base class to check whether 
            // this directory contains the executable file.
            // !!!!Obsolete when to-do from above is implemented.!!!!
            return new GameLocationData {Location = _directory};
        }
    }
}