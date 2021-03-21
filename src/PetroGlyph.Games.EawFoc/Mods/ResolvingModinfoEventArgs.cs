using System.ComponentModel;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class ResolvingModinfoEventArgs : CancelEventArgs
    {
        public IMod Mod { get; }

        public ResolvingModinfoEventArgs(IMod mod)
        {
            Mod = mod;
        }
    }
}