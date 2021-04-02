using System.IO;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Tests
{
    public class ModCreationTests
    {
        private static readonly string TestScenariosPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestScenarios"));
        private readonly IGame _game;

        public ModCreationTests()
        {
            _game = new Foc(new DirectoryInfo(Path.Combine(TestScenariosPath, "TwoMods")), GamePlatform.Disk);
        }

        [Fact]
        public void ModCreation()
        {
            var path = Path.Combine(_game.Directory.FullName, "Mods\\ModA");
            var mod = ModFactory.CreateMod(_game, ModType.Default, path, false);
            Assert.NotNull(mod);
            Assert.Equal(ModType.Default, mod.Type);
            Assert.IsType<Mod>(mod);
            Assert.Equal(path, ((Mod)mod).Directory.FullName);
        }

        [Fact]
        public void ModNotExists()
        {
            var path = Path.Combine(_game.Directory.FullName, "Mods\\ModC");
            Assert.Throws<ModException>(() =>
                ModFactory.CreateMod(_game, ModType.Default, path, false));
        }
    }
}