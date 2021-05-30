namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    /// <summary>
    /// Service that detects installed PG Star Wars game installations on this machine.
    /// </summary>
    public interface IGameDetector
    {

        GameDetectionResult Detect(GameDetectorOptions options);

        bool TryDetect(GameDetectorOptions options, out GameDetectionResult result);
    }
}