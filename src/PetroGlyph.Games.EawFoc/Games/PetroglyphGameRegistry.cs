using System;

namespace PetroGlyph.Games.EawFoc.Games
{
    public abstract class PetroglyphGameRegistry : RegistryHelper
    {
        private readonly Lazy<string> _unversionedRootPathLazy;

        protected string UnversionedRegistryBasePath => _unversionedRootPathLazy.Value;

        public bool Installed => HasValue(nameof(Installed));

        public bool Exists
        {
            get
            {
                using var key = RootRegistryKey.OpenSubKey(UnversionedRegistryBasePath);
                return key != null;
            }
        }

        public string? ExePath
        {
            get
            {
                GetValue(nameof(ExePath), out string path);
                return path;
            }
        }

        public string? InstallPath
        {
            get
            {
                GetValue(nameof(InstallPath), out string path);
                return path;
            }
        }

        protected PetroglyphGameRegistry()
        {
            _unversionedRootPathLazy =
                new Lazy<string>(() => RegistryBasePath.Substring(0, RegistryBasePath.LastIndexOf('\\')));
        }
    }
}