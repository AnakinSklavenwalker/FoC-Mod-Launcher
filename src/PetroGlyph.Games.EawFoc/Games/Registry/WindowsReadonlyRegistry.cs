using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Validation;

namespace PetroGlyph.Games.EawFoc.Games.Registry
{
    public class WindowsReadonlyRegistry : IDisposable
    {
        private readonly RegistryKey _rootKey;
        private readonly string _basePath;

        public WindowsReadonlyRegistry(RegistryKey rootKey, string? basePath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotSupportedException("This instance is only supported on windows systems");
            Requires.NotNull(rootKey, nameof(rootKey));
            _rootKey = rootKey;
            _basePath = basePath ?? string.Empty;
        }

#pragma warning disable CA1416 // Check Platform compatibility

        public bool GetValueOrDefault<T>(string name, string subPath, out T? result, T? defaultValue)
        {
            result = defaultValue;
            using var key = GetKey(subPath);
            var value = key?.GetValue(name, defaultValue);
            if (value is null)
                return false;

            try
            {
                result = (T)value;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GetValueOrDefault<T>(string name, out T? result, T? defaultValue)
        {
            return GetValueOrDefault(name, string.Empty, out result, defaultValue);
        }

        public bool HasPath(string path)
        {
            using var key = GetKey(path);
            return key != null;
        }

        public bool HasValue(string name)
        {
            return GetValue<object>(name, out _);
        }

        public bool GetValue<T>(string name, string subPath, out T? value)
        {
            return GetValueOrDefault(name, subPath, out value, default);
        }

        public bool GetValue<T>(string name, out T? value)
        {
            return GetValue(name, string.Empty, out value);
        }

        public RegistryKey? GetKey(string subPath, bool writable = false)
        {
#pragma warning disable IO0006 // Replace Path class with IFileSystem.Path for improved testability
            return _rootKey.OpenSubKey(Path.Combine(_basePath, subPath), writable);
#pragma warning restore IO0006 // Replace Path class with IFileSystem.Path for improved testability
        }

        public void Dispose()
        {
            _rootKey.Dispose();
        }
#pragma warning restore CA1416 // Check Platform compatibility
    }
}