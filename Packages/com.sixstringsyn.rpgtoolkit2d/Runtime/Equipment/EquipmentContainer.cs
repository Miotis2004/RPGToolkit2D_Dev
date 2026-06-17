using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Items;

namespace SixStringSyn.RPGToolkit2D.Runtime.Equipment
{
    public sealed class EquipmentContainer
    {
        private readonly Dictionary<string, ItemInstance> _equipped = new Dictionary<string, ItemInstance>(StringComparer.OrdinalIgnoreCase);
        public event Action<EquipmentContainer, EquipmentSlotDefinition, ItemInstance> EquippedChanged;
        public IReadOnlyDictionary<string, ItemInstance> EquippedItems => _equipped;

        public bool CanEquip(EquipmentSlotDefinition slot, ItemInstance item)
        {
            if (slot == null || item == null || item.Definition == null || item.Quantity != 1) return false;
            var allowed = item.Definition.AllowedEquipmentSlots;
            return allowed == null || allowed.Count == 0 || allowed.Any(id => string.Equals(id, slot.SlotId, StringComparison.OrdinalIgnoreCase));
        }

        public bool Equip(EquipmentSlotDefinition slot, ItemInstance item, out ItemInstance replaced)
        {
            replaced = null;
            if (!CanEquip(slot, item)) return false;
            _equipped.TryGetValue(slot.SlotId, out replaced);
            _equipped[slot.SlotId] = item;
            EquippedChanged?.Invoke(this, slot, item);
            return true;
        }

        public ItemInstance Unequip(EquipmentSlotDefinition slot)
        {
            if (slot == null || !_equipped.TryGetValue(slot.SlotId, out var item)) return null;
            _equipped.Remove(slot.SlotId);
            EquippedChanged?.Invoke(this, slot, null);
            return item;
        }
    }
}
