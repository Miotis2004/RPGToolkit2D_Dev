using System;
using SixStringSyn.RPGToolkit2D.Runtime.Core;

namespace SixStringSyn.RPGToolkit2D.Runtime.Stats
{
    public enum StatModifierType
    {
        Additive = 0,
        Multiplicative = 1,
        Override = 2
    }

    [Serializable]
    public sealed class StatModifier
    {
        public StatModifier(StatModifierType type, float value, int order = 0, bool isTemporary = false, RPGTag requiredTag = default)
        {
            Type = type;
            Value = value;
            Order = order;
            IsTemporary = isTemporary;
            RequiredTag = requiredTag;
        }

        public StatModifierType Type { get; }
        public float Value { get; }
        public int Order { get; }
        public bool IsTemporary { get; }
        public RPGTag RequiredTag { get; }
        public bool HasCondition => !RequiredTag.IsEmpty;
    }
}
