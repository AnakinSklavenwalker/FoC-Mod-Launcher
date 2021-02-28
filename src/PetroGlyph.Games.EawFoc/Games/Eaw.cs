using System;
using System.IO;
using System.Linq;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class Eaw : PetroglyphGame
    {
        public const string GameconstantsUpdateHashEaW = "1d44b0797c8becbe240adc0098c2302a";
        
        public override GameType Type { get; }

        public override string Name => "Star Wars: Empire at War";

        public override string Description => string.Empty;

        protected override int DefaultXmlFileCount => 1;

        protected override string GameExeFileName => "sweaw.exe";

        protected override string GameConstantsMd5Hash => GameconstantsUpdateHashEaW;

        public Eaw(DirectoryInfo gameDirectory, GameType gameType) : base(gameDirectory)
        {
            Type = gameType;
        }

        public override bool IsPatched()
        {
            if (!base.IsPatched())
                return false;
            return Directory.GetFiles(Path.Combine(Directory.FullName, @"Data\XML\")).Length == 1;
        }

        public override bool IsGameAiClear()
        {
            throw new NotSupportedException();
        }

        public override bool IsLanguageInstalled(string language)
        {
            return false;
        }

        // TODO: Add Origin
        // TODO: Put into game detection
        public static bool FindInstallationRelativeToFoc(FileInfo focExe, GameType type, out FileInfo? eawExe)
        {
            eawExe = null;
            switch (type)
            {
                case GameType.Disk:
                    var parent = focExe.Directory?.Parent;
                    var eawDir = parent?.GetDirectories().FirstOrDefault(x => x.Name.Equals("Star Wars Empire at War"));
                    if (eawDir is null)
                        return false;

                    var eawExePath = Path.Combine(eawDir.FullName, @"GameData\sweaw.exe");
                    if (!File.Exists(eawExePath))
                        return false;
                    eawExe = new FileInfo(eawExePath);
                    return true;
                case GameType.SteamGold:
                case GameType.GoG:
                    var eawDir2 = focExe.Directory?.Parent;
                    if (eawDir2 is null)
                        return false;
                    var eawExePath2 = Path.Combine(eawDir2.FullName, @"GameData\\sweaw.exe");
                    if (!File.Exists(eawExePath2))
                        return false;
                    eawExe = new FileInfo(eawExePath2);
                    break;
                case GameType.DiskGold:
                case GameType.Undefined:
                    return false;
            }
            return false;
        }
    }
}
