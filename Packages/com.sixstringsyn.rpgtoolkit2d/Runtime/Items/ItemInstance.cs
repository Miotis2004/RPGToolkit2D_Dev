using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;

namespace SixStringSyn.RPGToolkit2D.Runtime.Items
{
    [Serializable]
    public sealed class ItemInstance
    {
        private readonly Dictionary<string, string> _customState = new Dictionary<string, string>();
        private readonly List<StatModifier> _generatedStatModifiers = new List<StatModifier>();

        public ItemInstance(ItemDefinition definition, int quantity = 1, float durability = 1f)
        {
            Definition = definition;
            InstanceId = RPGId.NewId();
            Durability = Math.Max(0f, durability);
            Quantity = Math.Max(0, Math.Min(quantity, MaxStackSize));
        }

        public RPGId InstanceId { get; }
        public ItemDefinition Definition { get; }
        public int Quantity { get; private set; }
        public float Durability { get; set; }
        public IReadOnlyDictionary<string, string> CustomState => _customState;
        public IReadOnlyList<StatModifier> GeneratedStatModifiers => _generatedStatModifiers;
        public int MaxStackSize => Definition != null ? Definition.MaximumStackSize : 1;
        public bool IsEmpty => Definition == null || Quantity <= 0;
        public bool CanStackWith(ItemInstance other) => other != null && Definition == other.Definition && Durability.Equals(other.Durability);
        public int SpaceRemaining => Math.Max(0, MaxStackSize - Quantity);

        public int AddQuantity(int amount)
        {
            if (amount <= 0) return 0;
            var accepted = Math.Min(amount, SpaceRemaining);
            Quantity += accepted;
            return accepted;
        }

        public int RemoveQuantity(int amount)
        {
            if (amount <= 0) return 0;
            var removed = Math.Min(amount, Quantity);
            Quantity -= removed;
            return removed;
        }

        public ItemInstance Split(int amount)
        {
            var removed = RemoveQuantity(amount);
            return removed > 0 ? new ItemInstance(Definition, removed, Durability) : null;
        }

        public void SetCustomState(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key)) _customState[key] = value ?? string.Empty;
        }

        public void AddGeneratedStatModifier(StatModifier modifier)
        {
            if (modifier != null) _generatedStatModifiers.Add(modifier);
        }
    }
}
