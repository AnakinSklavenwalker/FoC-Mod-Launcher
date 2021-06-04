using System;
using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Searches a the current machine for a Petroglyph Star Wars Game by the most commonly wanted search heuristic.
    /// This Service is only available on Windows Systems
    /// 1. Searches the current directory of the running process for an game installation
    /// 2. Searches the Registry for a game installation
    /// </summary>
    public static class DefaultGameDetectorFactory
    {
        public static IGameDetector CreateDefaultDetector(IServiceProvider serviceProvider)
        {
            var currentDirDetector = DirectoryGameDetector.CurrentDirectoryGameDetector(serviceProvider);
            return new CompositeGameDetector(new List<IGameDetector> {currentDirDetector}, serviceProvider);
        }
    }
}