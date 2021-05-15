using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public interface IGameDetector
    {
        GameDetectionResult Detect(GameDetectorOptions options);

        bool TryDetect(GameDetectorOptions options, out GameDetectionResult result);
    }


    public record GameDetectorOptions(GameType Type)
    {
        public SortedSet<GamePlatform> TargetPlatforms { get; set; }
    }


    public class GameDetectionResult
    {
        public IGameIdentity GameIdentity { get; }

        public IDirectoryInfo? GameLocation { get; }

        public Exception? Error { get; }
    }
}