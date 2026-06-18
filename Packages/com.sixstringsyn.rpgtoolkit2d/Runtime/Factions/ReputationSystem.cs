using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;

namespace SixStringSyn.RPGToolkit2D.Runtime.Factions
{
    [Serializable] public sealed class ReputationModifier { public FactionDefinition faction; public int amount; public string reason; }
    [Serializable] public sealed class ReputationCondition { public FactionDefinition faction; public int minimumValue; public int maximumValue = int.MaxValue; public bool Evaluate(ReputationSystem reputation) => reputation != null && faction != null && reputation.Get(faction) >= minimumValue && reputation.Get(faction) <= maximumValue; }

    public sealed class ReputationSystem
    {
        private readonly Dictionary<string, int> _values = new Dictionary<string, int>();
        public event Action<FactionDefinition, int, int, string> ReputationChanged;
        public int Get(FactionDefinition faction) => faction == null ? 0 : (_values.TryGetValue(faction.Id.Value, out var value) ? faction.Clamp(value) : 0);
        public int Set(FactionDefinition faction, int value, string reason = null) { if (faction == null) return 0; var old = Get(faction); var next = faction.Clamp(value); _values[faction.Id.Value] = next; if (old != next) ReputationChanged?.Invoke(faction, old, next, reason ?? string.Empty); return next; }
        public int Apply(ReputationModifier modifier) => modifier == null ? 0 : Add(modifier.faction, modifier.amount, modifier.reason);
        public int Add(FactionDefinition faction, int amount, string reason = null) => Set(faction, Get(faction) + amount, reason);
        public bool Meets(ReputationCondition condition) => condition != null && condition.Evaluate(this);
    }
}
