using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection.Platform;
#if NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public abstract class GameDetector : IGameDetector
    {
        protected readonly IServiceProvider ServiceProvider;
        protected ILogger? Logger;
        protected IFileSystem FileSystem;

        protected GameDetector(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
            FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        }

        public GameDetectionResult Detect(GameDetectorOptions options)
        {
            options = options.Normalize();
            GameDetectionResult result = GameDetectionResult.NotInstalled(options.Type);
            try
            {
                var locationData = FindGameLocation(options.Type);
                if (!locationData.IsInstalled)
                {
                    Logger?.LogInformation($"Unable to find an installed game of type {options.Type}.");
                    return result;
                }

                var location = locationData.Location;
                var platform = new GamePlatformIdentifier(ServiceProvider).GetGamePlatform(options.Type, ref location);

                if (MatchesOptionsPlatform(options, platform))
                {
                    result = new GameDetectionResult(new GameIdentity(options.Type, platform), location,
                        !locationData.InitializationRequired);
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

        public bool TryDetect(GameDetectorOptions options, out GameDetectionResult result)
        {
            result = Detect(options);
            if (result.Error is not null)
                return false;
            return result.GameLocation is not null;
        }

        protected abstract GameLocationData FindGameLocation(GameType type);

        protected bool GameExeExists(IDirectoryInfo directory, GameType gameType)
        {
            var exeFile = gameType == GameType.EaW
                ? PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName
                : PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName;

            var exePath = FileSystem.Path.Combine(directory.FullName, exeFile);
            return FileSystem.File.Exists(exePath);
        }


        private static bool MatchesOptionsPlatform(GameDetectorOptions options, GamePlatform identifiedPlatform)
        {
            return options.TargetPlatforms.Contains(GamePlatform.Undefined) ||
                   options.TargetPlatforms.Contains(identifiedPlatform);
        }


        protected ref struct GameLocationData
        {
            public IDirectoryInfo? Location { get; init; }

            public bool InitializationRequired { get; init; }

#if NET
             [MemberNotNullWhen(true, nameof(Location))]
#endif
            public bool IsInstalled => Location != null;
        }
    }
}