using System.Collections.Generic;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public sealed class DictionaryDialogueContext : IDialogueContext
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
        public void Set(string key, string value) => _values[key] = value;
        public bool TryGetValue(string key, out string value) => _values.TryGetValue(key, out value);
    }
}
