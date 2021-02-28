using System;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class ModCollectionChangedEventArgs : EventArgs
    {
        public ModCollectionChangedAction Action { get; }

        public IMod Mod { get; }

        public ModCollectionChangedEventArgs(IMod mod, ModCollectionChangedAction action)
        {
            Mod = mod;
            Action = action;
        }
    }
}