using System;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    public interface IGameRegistry : IDisposable
    {
        event EventHandler<RegistryChangedEventArgs> RegistryValueChanged; 

        bool WatchChanges { get; set; }

        string KeyPath { get; }

        IGame? Game { get; }
        
        string? Version { get; }

        string? CdKey { get; }

        int? EaWGold { get; }

        IFileInfo? ExePath { get; }

        bool? Installed { get; }

        IDirectoryInfo? InstallPath { get; }

        IFileInfo? Launcher { get; }

        int? Revision { get; }

        void AssignGame(IGame? game);
    }
}