using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PetroGlyph.Games.EawFoc.Client.Arguments
{
    public abstract class GameArgument<T> : IGameArgument
    {
        private T? _value;
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract bool IsDebug { get; }

        object? IGameArgument.Value
        {
            get => Value;
            set
            {
                if (value is not T castedValue)
                    throw new InvalidCastException();
                Value = castedValue;
            }
        }

        public T? Value
        {
            get => _value;
            set
            {
                if (value is null && _value is null)
                    return;
                if (Equals(value, _value))
                    return;
                _value = value;
                OnPropertyChanged();
            }
        }

        public abstract string ToCommandLine();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}