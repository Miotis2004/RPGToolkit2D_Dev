using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Items;

namespace SixStringSyn.RPGToolkit2D.Runtime.Inventory
{
    [Serializable]
    public sealed class InventoryItemSaveData
    {
        public string itemId;
        public int quantity;
        public float durability;
    }

    [Serializable]
    public sealed class InventorySaveData
    {
        public List<InventoryItemSaveData> items = new List<InventoryItemSaveData>();

        public static InventorySaveData FromInventory(InventoryContainer inventory)
        {
            var data = new InventorySaveData();
            if (inventory == null) return data;
            foreach (var slot in inventory.Slots)
            {
                if (slot.IsEmpty) continue;
                data.items.Add(new InventoryItemSaveData { itemId = slot.Item.Definition.Id.Value, quantity = slot.Item.Quantity, durability = slot.Item.Durability });
            }
            return data;
        }

        public InventoryContainer ToInventory(int capacity, Func<RPGId, ItemDefinition> resolver)
        {
            var inventory = new InventoryContainer(capacity);
            if (resolver == null) return inventory;
            foreach (var item in items)
            {
                var definition = resolver(new RPGId(item.itemId));
                if (definition != null) inventory.Add(new ItemInstance(definition, item.quantity, item.durability));
            }
            return inventory;
        }
    }
}
