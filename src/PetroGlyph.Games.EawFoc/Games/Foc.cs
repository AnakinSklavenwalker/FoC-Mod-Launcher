using System.IO;
using System.Linq;
using PetroGlyph.Games.EawFoc.Client;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class Foc : PetroglyphGame
    {
        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";
        public const string GraphicdetailsUpdateHash = "4d7e140887fc1dd52f47790a6e20b5c5";

        public override GameType Type { get; }

        protected override string GameExeFileName => "swfoc.exe";
        protected override string GameConstantsMd5Hash => GameconstantsUpdateHash;

        protected override int DefaultXmlFileCount => 2;

        public override string Name => "Forces of Corruption";
        public override string Description => string.Empty;

        public override string? IconFile => LauncherApp.FocIconPath;

        public Foc(DirectoryInfo gameDirectory, GameType type) : base(gameDirectory)
        {
            Type = type;
        }

        public override bool IsPatched()
        {
            if (!base.IsPatched())
                return false;
            var graphicsFilePath = Path.Combine(Directory.FullName, @"Data\XML\GRAPHICDETAILS.XML");
            if (!File.Exists(graphicsFilePath))
                return false;
            var hashProvider = new HashProvider();
            return hashProvider.GetFileHash(graphicsFilePath) == GraphicdetailsUpdateHash;
        }

        protected override void OnGameStarting(GameStartingEventArgs args)
        {
            if (args.GameArguments.Mods is null || args.GameArguments.Mods.Any())
                return;
            base.OnGameStarting(args);
        }
    }
}
