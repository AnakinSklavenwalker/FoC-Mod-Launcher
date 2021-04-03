using System;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    public class RegistryChangedEventArgs : EventArgs
    {
        public string PropertyName { get; }

        public object? OldValue { get; }

        public object? NewValue { get; }

        public RegistryChangedEventArgs(string propertyName, object? oldValue, object? newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}