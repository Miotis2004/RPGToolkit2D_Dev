using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;

namespace SixStringSyn.RPGToolkit2D.Runtime.Stats
{
    public sealed class StatBlock
    {
        private readonly Dictionary<StatDefinition, float> _baseValues = new Dictionary<StatDefinition, float>();
        private readonly Dictionary<StatDefinition, List<StatModifier>> _modifiers = new Dictionary<StatDefinition, List<StatModifier>>();
        private readonly List<RPGTag> _contextTags = new List<RPGTag>();

        public event Action<StatDefinition, float, float> StatChanged;

        public StatBlock(IEnumerable<StatValue> values = null, IEnumerable<RPGTag> contextTags = null)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    SetBaseValue(value.Definition, value.BaseValue, false);
                }
            }

            if (contextTags != null)
            {
                _contextTags.AddRange(contextTags.Where(tag => !tag.IsEmpty));
            }
        }

        public float GetBaseValue(StatDefinition stat) => stat != null && _baseValues.TryGetValue(stat, out var value) ? value : stat != null ? stat.DefaultValue : 0f;

        public void SetBaseValue(StatDefinition stat, float value) => SetBaseValue(stat, value, true);

        public float GetValue(StatDefinition stat)
        {
            if (stat == null)
            {
                return 0f;
            }

            var result = GetBaseValue(stat);
            if (_modifiers.TryGetValue(stat, out var modifiers))
            {
                foreach (var modifier in modifiers.Where(IsModifierActive).OrderBy(modifier => modifier.Type).ThenBy(modifier => modifier.Order))
                {
                    switch (modifier.Type)
                    {
                        case StatModifierType.Additive:
                            result += modifier.Value;
                            break;
                        case StatModifierType.Multiplicative:
                            result *= modifier.Value;
                            break;
                        case StatModifierType.Override:
                            result = modifier.Value;
                            break;
                    }
                }
            }

            return stat.Clamp(result);
        }

        public void AddModifier(StatDefinition stat, StatModifier modifier)
        {
            if (stat == null || modifier == null)
            {
                return;
            }

            var before = GetValue(stat);
            if (!_modifiers.TryGetValue(stat, out var modifiers))
            {
                modifiers = new List<StatModifier>();
                _modifiers[stat] = modifiers;
            }

            modifiers.Add(modifier);
            RaiseIfChanged(stat, before);
        }

        public bool RemoveModifier(StatDefinition stat, StatModifier modifier)
        {
            if (stat == null || modifier == null || !_modifiers.TryGetValue(stat, out var modifiers))
            {
                return false;
            }

            var before = GetValue(stat);
            var removed = modifiers.Remove(modifier);
            if (removed)
            {
                RaiseIfChanged(stat, before);
            }

            return removed;
        }

        public int RemoveTemporaryModifiers()
        {
            var removed = 0;
            foreach (var pair in _modifiers.ToArray())
            {
                var before = GetValue(pair.Key);
                removed += pair.Value.RemoveAll(modifier => modifier.IsTemporary);
                RaiseIfChanged(pair.Key, before);
            }

            return removed;
        }

        public void SetContextTags(IEnumerable<RPGTag> tags)
        {
            var stats = _modifiers.Keys.ToArray();
            var before = stats.ToDictionary(stat => stat, GetValue);
            _contextTags.Clear();
            if (tags != null)
            {
                _contextTags.AddRange(tags.Where(tag => !tag.IsEmpty));
            }

            foreach (var stat in stats)
            {
                RaiseIfChanged(stat, before[stat]);
            }
        }

        private void SetBaseValue(StatDefinition stat, float value, bool notify)
        {
            if (stat == null)
            {
                return;
            }

            var before = notify ? GetValue(stat) : 0f;
            _baseValues[stat] = stat.Clamp(value);
            if (notify)
            {
                RaiseIfChanged(stat, before);
            }
        }

        private bool IsModifierActive(StatModifier modifier) => !modifier.HasCondition || RPGTagQuery.HasTag(_contextTags, modifier.RequiredTag);

        private void RaiseIfChanged(StatDefinition stat, float before)
        {
            var after = GetValue(stat);
            if (!before.Equals(after))
            {
                StatChanged?.Invoke(stat, before, after);
            }
        }
    }
}
