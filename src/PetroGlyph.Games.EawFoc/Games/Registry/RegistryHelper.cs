using System;
using System.IO;

namespace PetroGlyph.Games.EawFoc.Games
{
    public abstract class RegistryHelper
    {
        protected abstract string RegistryBasePath { get; }

        protected abstract RegistryKey RootRegistryKey { get; }


        public bool GetValueOrDefault<T>(string name, string subPath, out T result, T defaultValue)
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

        public bool GetValueOrDefault<T>(string name, out T result, T defaultValue)
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

        public bool GetValue<T>(string name, string subPath, out T value)
        {
            return GetValueOrDefault(name, subPath, out value, default);
        }

        public bool GetValue<T>(string name, out T value)
        {
            return GetValue(name, string.Empty, out value);
        }

        public bool WriteValue(string name, string subPath, object value, RegistryValueKind registryValueKind = RegistryValueKind.Unknown)
        {
            try
            {
                using var key = GetKey(subPath, true);
                if (key is null)
                    return false;
                key.SetValue(name, value, registryValueKind);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool WriteValue(string name, object value, RegistryValueKind registryValueKind = RegistryValueKind.Unknown)
        {
            return WriteValue(name, string.Empty, value, registryValueKind);
        }

        public bool DeleteValue(string name)
        {
            return DeleteValue(name, string.Empty);
        }

        public bool DeleteValue(string name, string subPath)
        {
            try
            {
                using var key = GetKey(subPath, true);
                key?.DeleteValue(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public RegistryKey? GetKey(string subPath, bool writable = false)
        {
            return RootRegistryKey.OpenSubKey(Path.Combine(RegistryBasePath, subPath), writable);
        }
    }
}