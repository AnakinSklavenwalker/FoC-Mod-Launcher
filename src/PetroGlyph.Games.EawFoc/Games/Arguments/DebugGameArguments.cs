using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Games.Arguments
{
    public record DebugGameArguments : GameArguments
    {
        public IDirectoryInfo? OriginalAssetPath { get; set; }

        public bool? IgnoreAsserts { get; set; }

        public bool? NoArtProcess { get; set; }

        public bool? Recording { get; set; }
        public bool? Multi { get; set; }
        public uint? RandomSeed { get; set; }

        public string? ExpCDKey { get; set; }
        public string? CDKey { get; set; }
        public bool? PersistAsserts { get; set; }
        public bool? NoTimeout { get; set; }
        public bool? SuppressLog { get; set; }

        public bool? PreValidateAudioMessages { get; set; }

        public bool? LeftLeft { get; set; }

        public IFileInfo? MpPlaybackFile { get; set; }

        public bool? DeepSync { get; set; }
        public bool? NoFow { get; set; }

        public IFileInfo? Map { get; set; }

        public bool? NoIntro { get; set; }

        public bool? SafeLoadDebug { get; set; }

        public bool? RefCountTracking { get; set; }

        public IDirectoryInfo? SaveFolder { get; set; }

        public object? QuickLoadRecording { get; set; }
        public object? QuickLoad { get; set; }

        public bool? NoHardwareMouse { get; set; }

        public bool? ProcessEnglishAssetsAndExit { get; set; }
        public bool? ProcessAssetsAndExit { get; set; }
        public bool? Attract { get; set; }

        public object? Config { get; set; }

        public bool? DebugUnits { get; set; }

        public uint? Profile { get; set; }
        public uint? BCAST4 { get; set; }
        public uint? BCAST2 { get; set; }
        public bool? NoMenu { get; set; }

        public object? AiLogStyle { get; set; }
        public IFileInfo? AiLogFile { get; set; }
        public IFileInfo? RandomLogFile { get; set; }
        public IFileInfo? LogFile { get; set; }
        public IFileInfo? ConsoleCommandFile { get; set; }

        public ushort? SyncLogFilter { get; set; }
        public int? ConnectPort { get; set; }
        public int? ConnectIP { get; set; }
    }
}