using System.IO;

namespace PetroGlyph.Games.EawFoc.Games
{
    public sealed class SteamGameEaw : SteamGameBase
    {
        public const int EmpireAtWarSteamId = 32470;
        public const string GameconstantsUpdateHash = "1d44b0797c8becbe240adc0098c2302a";

        public override string Name => "Empire at War (Steam)";
        public override string Description => string.Empty;
        protected override int DefaultXmlFileCount => 1;

        public override string? IconFile => LauncherApp.EawIconPath;

        protected override string GameConstantsMd5Hash => GameconstantsUpdateHash;

        public override GameType Type => GameType.SteamGold;

        public SteamGameEaw(DirectoryInfo gameDirectory) : base(gameDirectory)
        {
        }
    }
}