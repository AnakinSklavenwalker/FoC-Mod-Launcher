using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.Spec;
using EawModinfo.Spec.Steam;
using NuGet.Versioning;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Tests
{
    public class ModDependencyTests
    {
        private static readonly string TestScenariosPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestScenarios"));
        private readonly IMod _modA;
        private readonly IMod _modB;
        private readonly IMod _modC;
        private readonly IMod _modE;
        private readonly IMod _modD;

        public ModDependencyTests()
        {
            IGame game = new Foc(new DirectoryInfo(Path.Combine(TestScenariosPath, "FiveMods")), GameType.Disk);
            _modA = ModFactory.CreateMod(game, ModType.Default, Path.Combine(game.Directory.FullName, "Mods\\ModA"), false);
            _modB = ModFactory.CreateMod(game, ModType.Default, Path.Combine(game.Directory.FullName, "Mods\\ModB"), false);
            _modC = ModFactory.CreateMod(game, ModType.Default, Path.Combine(game.Directory.FullName, "Mods\\ModC"), false);
            _modD = ModFactory.CreateMod(game, ModType.Default, Path.Combine(game.Directory.FullName, "Mods\\ModD"), false);
            _modE = ModFactory.CreateMod(game, ModType.Default, Path.Combine(game.Directory.FullName, "Mods\\ModE"), false);
            game.AddMod(_modA);
            game.AddMod(_modB);
            game.AddMod(_modC);
            game.AddMod(_modD);
            game.AddMod(_modE);
        }

        [Fact]
        public void SingleDependency()
        {
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
            });

            Assert.Equal(1, _modA.ExpectedDependencies);
            Assert.True(_modA.DependenciesResolved);
            Assert.True(_modA.HasDependencies);
            Assert.Equal(_modA.ExpectedDependencies, _modA.Dependencies.Count);
        }
        
        [Fact]
        public void TestNoCycle()
        {
            // A : B, C
            // B : D
            // C : E
            // Expected list: (A,) B, C, D, E
            var expected = new List<IMod> { _modA, _modB, _modC, _modD, _modE };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                a.Dependencies.Add(_modC);
                b.Dependencies.Add(_modD);
                c.Dependencies.Add(_modE);
            });
            
            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            Assert.Equal(expected, mods.ToList());
            AssertRecursive(expected);
        }

        [Fact]
        public void TestNoCycle2()
        {


            // A : C, B
            // B : D
            // C : E
            // Expected list: (A,) C, B, E, D
            var expected = new List<IMod> { _modA, _modC, _modB, _modE, _modD };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modC);
                a.Dependencies.Add(_modB);
                b.Dependencies.Add(_modD);
                c.Dependencies.Add(_modE);
            });

            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            Assert.Equal(expected, mods.ToList());
            AssertRecursive(expected);
        }

        [Fact]
        public void TestNoCycle3()
        {
            // A : B, C
            // B : D
            // C : D
            // D : E
            // Actual List: (A,) B, C, D, D, E, E
            // Reduced Expected list: (A,) B, C, D, E
            var expected = new List<IMod> { _modA, _modB, _modC, _modD, _modE };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                a.Dependencies.Add(_modC);
                b.Dependencies.Add(_modD);
                c.Dependencies.Add(_modD);
                d.Dependencies.Add(_modE);
            });


            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            Assert.Equal(expected, mods.ToList());
            AssertRecursive(expected);
        }


        [Fact]
        public void TestNoCycle4()
        {
            // A : B, C, D
            // B : E
            // C : E
            // D : E
            // Actual List: (A,) B, C, D, E, E, E
            // Reduced Expected list: (A,) B, C, D, E
            var expected = new List<IMod> { _modA, _modB, _modC, _modD, _modE };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                a.Dependencies.Add(_modC);
                a.Dependencies.Add(_modD);
                b.Dependencies.Add(_modE);
                c.Dependencies.Add(_modE);
                d.Dependencies.Add(_modE);
            });

            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            Assert.Equal(expected, mods.ToList());
            AssertRecursive(expected);
        }

        [Fact]
        public void TestNoCycle5()
        {
            // A : B, C
            // B : E
            // C : D
            // E : D
            // Actual List: (A,) B, C, E, D, D
            // Reduced Expected list: (A,) B, C, E, D
            var expected = new List<IMod> { _modA, _modB, _modC, _modE, _modD };
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                a.Dependencies.Add(_modC);
                b.Dependencies.Add(_modE);
                c.Dependencies.Add(_modD);
                e.Dependencies.Add(_modD);
            });
            
            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            Assert.Equal(expected, mods.ToList());
            AssertRecursive(expected);
        }

        [Fact]
        public void TestCycleSelf()
        {
            // A : A
            // Cycle!
            Assert.Throws<ModException>(() =>
            {
                SetModInfo((a, b, c, d, e) =>
                {
                    a.Dependencies.Add(_modA);
                });
            });
            Assert.Throws<ModException>(() => AssertRecursive(null));
        }

        [Fact]
        public void TestCycle()
        {
            // A : B
            // B : A
            // Cycle!
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                b.Dependencies.Add(_modA);
            });
            
            var resolver = new ModDependencyTraverser(_modA);
            Assert.Throws<ModException>(() => resolver.Traverse());
            Assert.Throws<ModException>(() => AssertRecursive(null));
        }

        [Fact]
        public void TestCycleComlex()
        {
            // A : B
            // B : C, D
            // D : E
            // E : A
            // Cycle!
            SetModInfo((a, b, c, d, e) =>
            {
                a.Dependencies.Add(_modB);
                b.Dependencies.Add(_modC);
                b.Dependencies.Add(_modD);
                d.Dependencies.Add(_modE);
                e.Dependencies.Add(_modA);
            });

            var resolver = new ModDependencyTraverser(_modA);
            Assert.Throws<ModException>(() => resolver.Traverse());
            Assert.Throws<ModException>(() => AssertRecursive(null));
        }

        private void SetModInfo(Action<IModIdentity, IModIdentity, IModIdentity, IModIdentity, IModIdentity> setAction)
        {
            var modInfoA = new MyModinfo();
            var modInfoB = new MyModinfo();
            var modInfoC = new MyModinfo();
            var modInfoD = new MyModinfo();
            var modInfoE = new MyModinfo();

            setAction(modInfoA, modInfoB, modInfoC, modInfoD, modInfoE);

            ((ModBase)_modA).SetModInfo(modInfoA);
            ((ModBase)_modB).SetModInfo(modInfoB);
            ((ModBase)_modC).SetModInfo(modInfoC);
            ((ModBase)_modD).SetModInfo(modInfoD);
            ((ModBase)_modE).SetModInfo(modInfoE);

            _modA.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);
            _modB.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);
            _modC.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);
            _modD.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);
            _modE.ResolveDependencies(ModDependencyResolveStrategy.FromExistingMods);

        }

        private void ResetDependencies()
        {
            ((ModBase)_modA).ResetDependencies();
            ((ModBase)_modB).ResetDependencies();
            ((ModBase)_modC).ResetDependencies();
            ((ModBase)_modD).ResetDependencies();
            ((ModBase)_modE).ResetDependencies();
        }

        private void AssertRecursive(ICollection expected)
        {
            ResetDependencies();
            Assert.True(_modA.ResolveDependencies(ModDependencyResolveStrategy.FromExistingModsRecursive));
            var resolver = new ModDependencyTraverser(_modA);
            var mods = resolver.Traverse();
            Assert.Equal(expected, mods.ToList());
        }

        private class MyModinfo : IModinfo
        {
            public bool Equals(IModIdentity other)
            {
                throw new NotImplementedException();
            }

            public string Name { get; }
            public SemanticVersion? Version { get; }
            public IList<IModReference> Dependencies { get; }
            public string Summary { get; }
            public string Icon { get; }
            public IDictionary<string, object> Custom { get; }
            public ISteamData? SteamData { get; }
            public IEnumerable<ILanguageInfo> Languages { get; }

            public MyModinfo()
            {
                Dependencies = new List<IModReference>();
            }
        }
    }
}
