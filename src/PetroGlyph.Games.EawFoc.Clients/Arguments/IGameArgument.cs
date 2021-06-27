using System.ComponentModel;

namespace PetroGlyph.Games.EawFoc.Client.Arguments
{
    public interface IGameArgument : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }

        object? Value { get; set; }

        bool IsDebug { get; }

        string ToCommandLine();
    }
}