namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    public sealed class EaWRegistryHelper : PetroglyphGameRegistry
    {
        internal const string EawRegistryPath = @"SOFTWARE\LucasArts\Star Wars Empire at War\1.0";

        private static PetroglyphGameRegistry? _instance;

        public static PetroglyphGameRegistry Instance => _instance ??= new EaWRegistryHelper();
        
        protected override string RegistryBasePath => EawRegistryPath;

        protected override RegistryKey RootRegistryKey => Registry.LocalMachine;

        private EaWRegistryHelper()
        {
        }
    }
}