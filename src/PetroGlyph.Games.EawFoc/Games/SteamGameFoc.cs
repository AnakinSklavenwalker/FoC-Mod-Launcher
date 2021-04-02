using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Mods;

namespace PetroGlyph.Games.EawFoc.Games
{
    public sealed class SteamGameFoc : SteamGameBase
    {
        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";
        public const int ForcesOfCorruptionSteamId = 32472;
        
        protected override string GameConstantsMd5Hash => GameconstantsUpdateHash;

        protected override int DefaultXmlFileCount => 1;

        public override GamePlatform Platform => GamePlatform.SteamGold;

        public override string Name => "Forces of Corruption (Steam)";

        public override string IconFile => LauncherApp.FocIconPath;

        public override string Description => string.Empty;
        
        public SteamGameFoc(DirectoryInfo gameDirectory) : base(gameDirectory)
        {
        }
        
        protected override IEnumerable<IMod> GetPhysicalModsCore()
        {
            return SearchSteamMods().ToList();
        }

        private IEnumerable<IMod> SearchSteamMods()
        {
            var mods = new List<IMod>();
            mods.AddRange(SearchDiskMods());

            var workshopsPath = FileUtilities.NormalizePath(Path.Combine(Directory.FullName, @"..\..\..\workshop\content\32470\"));
            var workshopsDir = new DirectoryInfo(workshopsPath);
            if (!workshopsDir.Exists)
                return mods;

            var modDirs = workshopsDir.EnumerateDirectories();
            var workshopMods = modDirs.SelectMany(folder => ModFactory.CreateModAndVariants(this, ModType.Workshops, folder, true));

            mods.AddRange(workshopMods);
            return mods;
        }
    }
}
