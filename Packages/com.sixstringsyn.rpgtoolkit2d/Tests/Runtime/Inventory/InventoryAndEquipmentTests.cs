using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Equipment;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Inventory
{
    public sealed class InventoryAndEquipmentTests
    {
        [Test]
        public void InventoryStacksAndReportsOverflow()
        {
            var potion = Item(5);
            var inventory = new InventoryContainer(2);
            Assert.That(inventory.Add(potion, 12), Is.EqualTo(10));
            Assert.That(inventory.Count(potion), Is.EqualTo(10));
            Object.DestroyImmediate(potion);
        }

        [Test]
        public void InventorySplitsMergesAndRemovesItems()
        {
            var herb = Item(10);
            var inventory = new InventoryContainer(3);
            inventory.Add(herb, 8);
            var split = inventory.Split(0, 3);
            Assert.That(split.Quantity, Is.EqualTo(3));
            Assert.That(inventory.Add(split), Is.EqualTo(3));
            Assert.That(inventory.Remove(herb, 6), Is.EqualTo(6));
            Assert.That(inventory.Count(herb), Is.EqualTo(2));
            Object.DestroyImmediate(herb);
        }

        [Test]
        public void EquipmentRespectsSlotRestrictions()
        {
            var sword = Item(1);
            Set(sword, "_allowedEquipmentSlots", new System.Collections.Generic.List<string> { "main_hand" });
            var slot = ScriptableObject.CreateInstance<EquipmentSlotDefinition>();
            Set(slot, "_slotId", "main_hand");
            var equipment = new EquipmentContainer();
            Assert.That(equipment.Equip(slot, new ItemInstance(sword), out _), Is.True);
            Object.DestroyImmediate(sword);
            Object.DestroyImmediate(slot);
        }

        [Test]
        public void InventorySaveDataRoundTripsItemState()
        {
            var gem = Item(20);
            gem.SetId(new SixStringSyn.RPGToolkit2D.Runtime.Core.RPGId("item.gem"));
            var inventory = new InventoryContainer(4);
            inventory.Add(new ItemInstance(gem, 7, 0.5f));

            var data = InventorySaveData.FromInventory(inventory);
            var restored = data.ToInventory(4, id => id == gem.Id ? gem : null);

            Assert.That(restored.Count(gem), Is.EqualTo(7));
            Assert.That(restored.Slots[0].Item.Durability, Is.EqualTo(0.5f));
            Object.DestroyImmediate(gem);
        }

        private static ItemDefinition Item(int stack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            Set(item, "_maximumStackSize", stack);
            return item;
        }

        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
