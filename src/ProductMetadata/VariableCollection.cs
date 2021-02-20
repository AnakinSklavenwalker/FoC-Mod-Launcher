using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Validation;

namespace ProductMetadata
{
    public class VariableCollection : IReadOnlyDictionary<string, string?>
    {
        internal static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
        private readonly ReaderWriterLockSlim _collectionLock = new();
        private readonly IDictionary<string, Variable> _variables;

        public IEnumerable<string> Keys
        {
            get
            {
                _collectionLock.EnterReadLock();
                try
                {
                    return _variables.Keys;
                }
                finally
                {
                    _collectionLock.ExitReadLock();
                }
            }
        }

        public IEnumerable<string?> Values
        {
            get
            {
                _collectionLock.EnterReadLock();
                try
                {
                    return _variables.Select(v => v.Value.Value);
                }
                finally
                {
                    _collectionLock.ExitReadLock();
                }
            }
        }

        public int Count
        {
            get
            {
                _collectionLock.EnterReadLock();
                try
                {
                    return _variables.Count;
                }
                finally
                {
                    _collectionLock.ExitReadLock();
                }
            }
        }


        public string? this[string name]
        {
            get
            {
                Requires.NotNullOrEmpty(name, nameof(name));
                return Get(name);
            }
            set
            {
                Requires.NotNullOrEmpty(name, nameof(name));
                _collectionLock.EnterUpgradeableReadLock();
                try
                {
                    if (_variables.TryGetValue(name, out var userValue))
                    {
                        if (value != null && value.Equals(userValue.Value, StringComparison.OrdinalIgnoreCase))
                            return;
                        _collectionLock.EnterWriteLock();
                        try
                        {
                            _variables.Remove(name);
                        }
                        finally
                        {
                            _collectionLock.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _collectionLock.ExitUpgradeableReadLock();
                }
                if (value == null)
                    return;
                Add(name, value);
            }
        }


        public VariableCollection()
        {
            _variables = new Dictionary<string, Variable>();
        }

        public bool Contains(string name)
        {
            Requires.NotNullOrEmpty(name, nameof(name));
            _collectionLock.EnterReadLock();
            try
            {
                return _variables.ContainsKey(name);
            }
            finally
            {
                _collectionLock.ExitReadLock();
            }
        }

        public string? Get(string name, string? defaultValue = null)
        {
            Requires.NotNullOrEmpty(name, nameof(name));
            _collectionLock.EnterReadLock();
            try
            {
                string? str = null;
                if (_variables.ContainsKey(name))
                    str = _variables[name].Value;
                return str ?? defaultValue;
            }
            finally
            {
                _collectionLock.ExitReadLock();
            }
        }

        public void Add(string name, string? value)
        {
            Requires.NotNullOrEmpty(name, nameof(name));
            _collectionLock.EnterUpgradeableReadLock();
            try
            {
                if (value is null)
                    return;
                var variable = new Variable(name, value);
                if (_variables.ContainsKey(name))
                {
                    if (value.Equals(_variables[name].Value, StringComparison.OrdinalIgnoreCase))
                        return;
                    _collectionLock.EnterWriteLock();
                    try
                    {
                        _variables[name] = variable;
                    }
                    finally
                    {
                        _collectionLock.ExitWriteLock();
                    }
                }
                else
                {
                    _collectionLock.EnterWriteLock();
                    try
                    {
                        _variables.Add(name, variable);
                    }
                    finally
                    {
                        _collectionLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _collectionLock.ExitUpgradeableReadLock();
            }
        }

        public IDictionary<string, string?> ToDictionary()
        {
            _collectionLock.EnterReadLock();
            try
            {
                Dictionary<string, string?> dictionary = new(Comparer);
                foreach (var keyValuePair in this)
                    dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                return dictionary;
            }
            finally
            {
                _collectionLock.ExitReadLock();
            }
        }

        internal void Add(string name, Func<string> initialize)
        {
            Requires.NotNullOrEmpty(name, nameof(name));
            Requires.NotNull(initialize, nameof(initialize));
            Variable variable = new(name, initialize);
            _collectionLock.EnterWriteLock();
            try
            {
                if (_variables.ContainsKey(name))
                {
                    if (string.Equals(variable.Value, _variables[name].Value, StringComparison.OrdinalIgnoreCase))
                        return;
                    _variables[name] = variable;
                }
                else
                    _variables.Add(name, variable);
            }
            finally
            {
                _collectionLock.ExitWriteLock();
            }
        }

        bool IReadOnlyDictionary<string, string?>.ContainsKey(string key) => Contains(key);

        public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        {
            foreach (var variable in _variables.Values)
                yield return variable.GetPair();
        }

        bool IReadOnlyDictionary<string, string?>.TryGetValue(string key, out string? value)
        {
            value = null;
            if (!Contains(key))
                return false;
            value = Get(key);
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Variable
        {
            private string? _value;
            private bool _initialized;

            private string Name { get; }

            private Func<string>? Initialize { get; }

            internal string? Value
            {
                get
                {
                    if (!_initialized)
                    {
                        _value = Initialize?.Invoke();
                        _initialized = true;
                    }
                    return _value;
                }
            }

            internal Variable(string name, string value)
            {
                Name = name;
                _initialized = true;
                _value = value;
            }

            internal Variable(string name, Func<string> initialize)
            {
                Name = name;
                Initialize = initialize;
            }

            internal KeyValuePair<string, string?> GetPair() => new(Name, Value);
        }
    }
}