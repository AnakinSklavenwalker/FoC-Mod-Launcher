using System;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Service that detects installed PG Star Wars game installations on this machine.
    /// </summary>
    public interface IGameDetector
    {
        event EventHandler<GameInitializeRequestEventArgs> InitializationRequested;

        GameDetectionResult Detect(GameDetectorOptions options);

        bool TryDetect(GameDetectorOptions options, out GameDetectionResult result);
    }

    public class GameInitializeRequestEventArgs : EventArgs
    {
        public GameDetectorOptions Options { get; }

        public bool Handled { get; set; }

        public GameInitializeRequestEventArgs(GameDetectorOptions options)
        {
            Options = options;
        }
    }
}