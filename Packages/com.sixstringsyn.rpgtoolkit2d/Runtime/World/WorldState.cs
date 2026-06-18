using System;
using System.Collections.Generic;

namespace SixStringSyn.RPGToolkit2D.Runtime.World
{
    [Serializable]
    public sealed class WorldStateEntry { public string key; public string value; }

    [Serializable]
    public sealed class WorldStateSaveData { public List<WorldStateEntry> entries = new List<WorldStateEntry>(); }

    public sealed class WorldState
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
        public event Action<string> Changed;
        public IReadOnlyDictionary<string, string> Values => _values;
        public bool Has(string key) => !string.IsNullOrWhiteSpace(key) && _values.ContainsKey(key);
        public void SetFlag(string key, bool value = true) => SetString(key, value ? "true" : "false");
        public bool GetFlag(string key, bool fallback = false) => TryGet(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;
        public void SetCounter(string key, int value) => SetString(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        public int GetCounter(string key, int fallback = 0) => TryGet(key, out var value) && int.TryParse(value, out var parsed) ? parsed : fallback;
        public void AddCounter(string key, int amount) => SetCounter(key, GetCounter(key) + amount);
        public void SetVariable(string key, float value) => SetString(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        public float GetVariable(string key, float fallback = 0f) => TryGet(key, out var value) && float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;
        public void SetString(string key, string value) { if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("World state key is required.", nameof(key)); _values[key] = value ?? string.Empty; Changed?.Invoke(key); }
        public bool TryGet(string key, out string value) => _values.TryGetValue(key, out value);
        public bool Clear(string key) { var removed = _values.Remove(key); if (removed) Changed?.Invoke(key); return removed; }
        public void ClearAll() { foreach (var key in new List<string>(_values.Keys)) Clear(key); }
        public WorldStateSaveData Capture() { var data = new WorldStateSaveData(); foreach (var pair in _values) data.entries.Add(new WorldStateEntry { key = pair.Key, value = pair.Value }); return data; }
        public void Restore(WorldStateSaveData data) { _values.Clear(); if (data == null) return; foreach (var entry in data.entries) if (!string.IsNullOrWhiteSpace(entry.key)) _values[entry.key] = entry.value ?? string.Empty; }
    }

    public enum WorldStateComparison { Exists, NotExists, Equals, NotEquals, GreaterOrEqual, LessOrEqual }

    [Serializable]
    public sealed class WorldStateCondition
    {
        public string key; public WorldStateComparison comparison = WorldStateComparison.Exists; public string value;
        public bool Evaluate(WorldState state)
        {
            if (state == null) return false;
            var exists = state.TryGet(key, out var current);
            switch (comparison)
            {
                case WorldStateComparison.Exists: return exists;
                case WorldStateComparison.NotExists: return !exists;
                case WorldStateComparison.Equals: return exists && string.Equals(current, value, StringComparison.OrdinalIgnoreCase);
                case WorldStateComparison.NotEquals: return !exists || !string.Equals(current, value, StringComparison.OrdinalIgnoreCase);
                case WorldStateComparison.GreaterOrEqual: return exists && float.TryParse(current, out var c1) && float.TryParse(value, out var v1) && c1 >= v1;
                case WorldStateComparison.LessOrEqual: return exists && float.TryParse(current, out var c2) && float.TryParse(value, out var v2) && c2 <= v2;
                default: return false;
            }
        }
    }
}
