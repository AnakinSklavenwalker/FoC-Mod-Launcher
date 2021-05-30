using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    internal abstract class GamePlatformValidator : IGamePlatformValidator
    {
        protected readonly ILogger? Logger;

        protected GamePlatformValidator(IServiceProvider serviceProvider)
        {
            Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        }

        public bool IsPlatform(GameType type, ref IDirectoryInfo location)
        {
            return false;
        }

        public abstract bool IsPlatformFoc(ref IDirectoryInfo location);
        public abstract bool IsPlatformEaw(ref IDirectoryInfo location);
    }
}