﻿using Microsoft.Extensions.Logging;

namespace PetroGlyph.Games.EawFoc.Services.Games.Detection
{
    public abstract class GameDetector
    {
        protected ILogger? Logger;

        public GameDetection DetectGames()
        {
            return DetectGamesCore();
        }

        protected abstract GameDetection DetectGamesCore();
    }
}