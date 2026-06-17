using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Items;

namespace SixStringSyn.RPGToolkit2D.Runtime.Inventory
{
    public sealed class InventoryContainer
    {
        private readonly List<InventorySlot> _slots;
        public event Action<InventoryContainer> Changed;
        public event Action<InventoryContainer, InventorySlot> SlotChanged;

        public InventoryContainer(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            _slots = Enumerable.Range(0, capacity).Select(i => new InventorySlot(i)).ToList();
        }

        public IReadOnlyList<InventorySlot> Slots => _slots;
        public int Capacity => _slots.Count;
        public InventorySlot this[int index] => _slots[index];

        public int Add(ItemDefinition definition, int quantity = 1) => Add(new ItemInstance(definition, quantity));

        public int Add(ItemInstance item)
        {
            if (item == null || item.IsEmpty) return 0;
            var remaining = item.Quantity;
            foreach (var slot in _slots.Where(s => !s.IsEmpty && s.Item.CanStackWith(item)))
            {
                remaining -= slot.Item.AddQuantity(remaining);
                Notify(slot);
                if (remaining == 0) return item.Quantity;
            }

            foreach (var slot in _slots.Where(s => s.IsEmpty))
            {
                var placed = Math.Min(remaining, item.MaxStackSize);
                slot.Item = new ItemInstance(item.Definition, placed, item.Durability);
                foreach (var state in item.CustomState) slot.Item.SetCustomState(state.Key, state.Value);
                remaining -= placed;
                Notify(slot);
                if (remaining == 0) break;
            }

            return item.Quantity - remaining;
        }

        public int Remove(ItemDefinition definition, int quantity)
        {
            if (definition == null || quantity <= 0) return 0;
            var remaining = quantity;
            foreach (var slot in _slots.Where(s => !s.IsEmpty && s.Item.Definition == definition).ToArray())
            {
                remaining -= slot.Item.RemoveQuantity(remaining);
                if (slot.Item.IsEmpty) slot.Clear();
                Notify(slot);
                if (remaining == 0) break;
            }
            return quantity - remaining;
        }

        public bool Move(int fromIndex, int toIndex)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex) || fromIndex == toIndex) return false;
            var from = _slots[fromIndex]; var to = _slots[toIndex];
            if (from.IsEmpty) return false;
            if (to.IsEmpty) { to.Item = from.Item; from.Clear(); Notify(from); Notify(to); return true; }
            if (to.Item.CanStackWith(from.Item))
            {
                var moved = to.Item.AddQuantity(from.Item.Quantity);
                from.Item.RemoveQuantity(moved);
                if (from.Item.IsEmpty) from.Clear();
                Notify(from); Notify(to); return moved > 0;
            }
            (from.Item, to.Item) = (to.Item, from.Item); Notify(from); Notify(to); return true;
        }

        public ItemInstance Split(int slotIndex, int amount)
        {
            if (!IsValidIndex(slotIndex) || _slots[slotIndex].IsEmpty) return null;
            var split = _slots[slotIndex].Item.Split(amount);
            if (_slots[slotIndex].Item.IsEmpty) _slots[slotIndex].Clear();
            Notify(_slots[slotIndex]);
            return split;
        }

        public bool Merge(int sourceIndex, int targetIndex) => Move(sourceIndex, targetIndex);
        public int Count(ItemDefinition definition) => _slots.Where(s => !s.IsEmpty && s.Item.Definition == definition).Sum(s => s.Item.Quantity);
        public bool Contains(ItemDefinition definition, int quantity = 1) => Count(definition) >= quantity;
        private bool IsValidIndex(int index) => index >= 0 && index < _slots.Count;
        private void Notify(InventorySlot slot) { SlotChanged?.Invoke(this, slot); Changed?.Invoke(this); }
    }
}
