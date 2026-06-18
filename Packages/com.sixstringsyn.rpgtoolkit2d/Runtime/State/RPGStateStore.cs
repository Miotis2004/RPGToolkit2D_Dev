using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.State
{
    public enum RPGStateValueType { Switch, Flag, Integer, Float, String }

    [Serializable]
    public sealed class RPGStateKeyDefinition
    {
        public string key;
        public RPGStateValueType valueType = RPGStateValueType.Flag;
        public string defaultValue;
        public bool isLocal;
    }

    [CreateAssetMenu(fileName = "RPGStateKeyDatabase", menuName = "RPG Toolkit/State/State Key Database")]
    public sealed class RPGStateKeyDatabase : ScriptableObject
    {
        [SerializeField] private List<RPGStateKeyDefinition> _keys = new List<RPGStateKeyDefinition>();
        public IReadOnlyList<RPGStateKeyDefinition> Keys => _keys;
        public void ApplyDefaults(RPGStateStore store) { if (store == null) return; foreach (var key in _keys) if (key != null && !string.IsNullOrWhiteSpace(key.key) && !store.Has(key.key, key.isLocal)) store.SetRaw(key.key, key.defaultValue, key.isLocal); }
        public RPGValidationResult Validate()
        {
            var result = new RPGValidationResult();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in _keys)
            {
                if (key == null || string.IsNullOrWhiteSpace(key.key)) { result.AddError("RPGSTATE_EMPTY_KEY", "State key database contains an empty key."); continue; }
                var scoped = (key.isLocal ? "local:" : "global:") + key.key;
                if (!seen.Add(scoped)) result.AddError("RPGSTATE_DUPLICATE_KEY", $"State key {key.key} is declared more than once.");
            }
            return result;
        }
    }

    public enum RPGStateComparison { Exists, NotExists, Equals, NotEquals, GreaterOrEqual, LessOrEqual, IsTrue, IsFalse }

    [Serializable]
    public sealed class RPGStateCondition
    {
        public string key;
        public bool local;
        public RPGStateComparison comparison = RPGStateComparison.Exists;
        public string value;
        public bool Evaluate(RPGStateStore store) => store != null && store.Evaluate(key, comparison, value, local);
    }

    public sealed class RPGStateStore
    {
        private readonly Dictionary<string, string> _global = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _local = new Dictionary<string, string>();
        public event Action<string, bool, string> Changed;
        public IReadOnlyDictionary<string, string> GlobalValues => _global;
        public IReadOnlyDictionary<string, string> LocalValues => _local;
        public bool Has(string key, bool local = false) => !string.IsNullOrWhiteSpace(key) && Map(local).ContainsKey(key);
        public void SetSwitch(string key, bool value, bool local = false) => SetRaw(key, value ? "true" : "false", local);
        public bool GetSwitch(string key, bool fallback = false, bool local = false) => TryGetRaw(key, out var value, local) && bool.TryParse(value, out var parsed) ? parsed : fallback;
        public void SetInt(string key, int value, bool local = false) => SetRaw(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture), local);
        public int GetInt(string key, int fallback = 0, bool local = false) => TryGetRaw(key, out var value, local) && int.TryParse(value, out var parsed) ? parsed : fallback;
        public void SetFloat(string key, float value, bool local = false) => SetRaw(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture), local);
        public float GetFloat(string key, float fallback = 0f, bool local = false) => TryGetRaw(key, out var value, local) && float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;
        public string GetString(string key, string fallback = "", bool local = false) => TryGetRaw(key, out var value, local) ? value : fallback;
        public void SetString(string key, string value, bool local = false) => SetRaw(key, value, local);
        public void SetRaw(string key, string value, bool local = false) { if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("State key is required.", nameof(key)); Map(local)[key] = value ?? string.Empty; Changed?.Invoke(key, local, value ?? string.Empty); }
        public bool TryGetRaw(string key, out string value, bool local = false) => Map(local).TryGetValue(key, out value);
        public bool Clear(string key, bool local = false) { var removed = Map(local).Remove(key); if (removed) Changed?.Invoke(key, local, null); return removed; }
        public bool Evaluate(string key, RPGStateComparison comparison, string value = null, bool local = false)
        {
            var exists = TryGetRaw(key, out var current, local);
            switch (comparison)
            {
                case RPGStateComparison.Exists: return exists;
                case RPGStateComparison.NotExists: return !exists;
                case RPGStateComparison.Equals: return exists && string.Equals(current, value, StringComparison.OrdinalIgnoreCase);
                case RPGStateComparison.NotEquals: return !exists || !string.Equals(current, value, StringComparison.OrdinalIgnoreCase);
                case RPGStateComparison.IsTrue: return exists && bool.TryParse(current, out var yes) && yes;
                case RPGStateComparison.IsFalse: return !exists || (bool.TryParse(current, out var no) && !no);
                case RPGStateComparison.GreaterOrEqual: return exists && Compare(current, value) >= 0;
                case RPGStateComparison.LessOrEqual: return exists && Compare(current, value) <= 0;
                default: return false;
            }
        }
        private static int Compare(string current, string expected) => float.TryParse(current, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var c) && float.TryParse(expected, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var e) ? c.CompareTo(e) : -1;
        private Dictionary<string, string> Map(bool local) => local ? _local : _global;
    }
}
