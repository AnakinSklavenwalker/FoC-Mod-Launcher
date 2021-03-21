using System;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class ModinfoResolvedEventArgs : EventArgs
    {
        public IMod Mod { get; }
        public IModinfo? Modinfo { get; }

        public ModinfoResolvedEventArgs(IMod mod, IModinfo? modinfo)
        {
            Mod = mod;
            Modinfo = modinfo;
        }
    }
}