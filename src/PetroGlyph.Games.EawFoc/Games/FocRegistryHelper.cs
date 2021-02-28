namespace PetroGlyph.Games.EawFoc.Games
{
    public sealed class FocRegistryHelper : PetroglyphGameRegistry
    {
        internal const string FocRegistryPath = @"SOFTWARE\LucasArts\Star Wars Empire at War Forces of Corruption\1.0";

        private static PetroglyphGameRegistry? _instance;

        public static PetroglyphGameRegistry Instance => _instance ??= new FocRegistryHelper();

        protected override string RegistryBasePath => FocRegistryPath;

        protected override RegistryKey RootRegistryKey => Registry.LocalMachine;

        private FocRegistryHelper()
        {
        }
    }
}