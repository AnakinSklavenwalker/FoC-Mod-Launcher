using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection.Platform;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public abstract class GameDetector : IGameDetector
    {
        public event EventHandler<GameInitializeRequestEventArgs>? InitializationRequested;

        protected readonly IServiceProvider ServiceProvider;
        private readonly bool _tryHandleInitialization;
        protected ILogger? Logger;
        protected IFileSystem FileSystem;

        protected GameDetector(IServiceProvider serviceProvider, bool tryHandleInitialization)
        {
            ServiceProvider = serviceProvider;
            _tryHandleInitialization = tryHandleInitialization;
            Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
            FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        }

        public static bool GameExeExists(IDirectoryInfo directory, GameType gameType)
        {
            if (!directory.Exists)
                return false;

            var exeFile = gameType == GameType.EaW
                ? PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName
                : PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName;

            var exePath = directory.FileSystem.Path.Combine(directory.FullName, exeFile);
            return directory.FileSystem.File.Exists(exePath);
        }

        public GameDetectionResult Detect(GameDetectorOptions options)
        {
            options = options.Normalize();
            GameDetectionResult result = GameDetectionResult.NotInstalled(options.Type);
            try
            {
                var locationData = FindGameLocation(options);
                locationData.ThrowIfInvalid();

                if (!locationData.IsInstalled)
                {
                    Logger?.LogInformation($"Unable to find an installed game of type {options.Type}.");
                    return result;
                }

                if (!HandleInitialization(options, ref locationData))
                    return GameDetectionResult.RequiresInitialization(options.Type);

#if DEBUG
                if (locationData.Location is null)
                    throw new InvalidOperationException("Illegal operation state: Expected location to be not null!");
#endif

                var location = locationData.Location!;
                var platform = new GamePlatformIdentifier(options.TargetPlatforms, ServiceProvider)
                    .GetGamePlatform(options.Type, ref location);

                if (!GameExeExists(location, options.Type))
                {
                    Logger?.LogDebug($"Unable to find any game executables at the given location: {location.FullName}");
                    return GameDetectionResult.NotInstalled(options.Type);
                }

                if (MatchesOptionsPlatform(options, platform))
                {
                    result = new GameDetectionResult(new GameIdentity(options.Type, platform), location);
                    Logger?.LogInformation($"Game detected: {result.GameIdentity} at location: {location.FullName}");
                    return result;
                }

                Logger?.LogInformation($"Game detected at location: {result.GameLocation?.FullName} " +
                                       $"but Platform {platform} was not requested.");
                return result;
            }
            catch (Exception e)
            {
                Logger?.LogWarning(e, "Unable to find any games, due to error in detection"); 
                return new GameDetectionResult(options.Type, e);
            }
        }

        private bool HandleInitialization(GameDetectorOptions options, ref GameLocationData locationData)
        {
            if (!locationData.InitializationRequired)
                return true;

            Logger?.LogInformation("The games seems to exists but is not initialized.");
            if (!_tryHandleInitialization)
                return false;

            Logger?.LogInformation("Calling event handler to initialize and try to get location again....");
            if (RequestInitialization(options))
                locationData = FindGameLocation(options);

            return locationData.Location is not null;
        }

        public bool TryDetect(GameDetectorOptions options, out GameDetectionResult result)
        {
            result = Detect(options);
            if (result.Error is not null)
                return false;
            return result.GameLocation is not null;
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        private protected abstract GameLocationData FindGameLocation(GameDetectorOptions options);
        

        private static bool MatchesOptionsPlatform(GameDetectorOptions options, GamePlatform identifiedPlatform)
        {
            return options.TargetPlatforms.Contains(GamePlatform.Undefined) ||
                   options.TargetPlatforms.Contains(identifiedPlatform);
        }


        public ref struct GameLocationData
        {
            public IDirectoryInfo? Location { get; init; }

            public bool InitializationRequired { get; init; }

            public bool IsInstalled => Location != null || InitializationRequired;

            internal void ThrowIfInvalid()
            {
                if (Location is not null && InitializationRequired)
                    throw new NotSupportedException($"The LocationData cannot have a location set " +
                                                    $"but also {nameof(InitializationRequired)} set to true.");
            }
        }

        private bool RequestInitialization(GameDetectorOptions options)
        {
            var request = new GameInitializeRequestEventArgs(options);
            InitializationRequested?.Invoke(this, request);
            return request.Handled;
        }
    }
}