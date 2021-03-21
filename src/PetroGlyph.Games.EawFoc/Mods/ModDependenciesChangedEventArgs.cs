using System;
using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class ModDependenciesChangedEventArgs : EventArgs
    {
        public IMod Mod { get; }
        public IList<IMod> OldDependencies { get; }
        public IList<IMod> NewDependencies { get; }

        public ModDependenciesChangedEventArgs(
            IMod mod, 
            IList<IMod> oldDependencies,
            IList<IMod> newDependencies)
        {
            Mod = mod;
            OldDependencies = oldDependencies;
            NewDependencies = newDependencies;
        }
    }
}