using System;

namespace SixStringSyn.RPGToolkit2D.Runtime.Stats
{
    [Serializable]
    public struct StatValue
    {
        public StatValue(StatDefinition definition, float baseValue)
        {
            Definition = definition;
            BaseValue = baseValue;
        }

        public StatDefinition Definition { get; }
        public float BaseValue { get; set; }
    }
}
