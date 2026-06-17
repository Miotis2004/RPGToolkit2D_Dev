using System;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    public sealed class InventorySaveContributor : ISaveContributor
    {
        private readonly Func<InventoryContainer> _getter;
        private readonly Action<InventoryContainer> _setter;
        private readonly Func<Core.RPGId, Items.ItemDefinition> _resolver;
        private readonly int _capacity;
        public InventorySaveContributor(Func<InventoryContainer> getter, Action<InventoryContainer> setter, Func<Core.RPGId, Items.ItemDefinition> resolver, int capacity, string systemId = "inventory")
        { _getter = getter; _setter = setter; _resolver = resolver; _capacity = capacity; SystemId = systemId; }
        public string SystemId { get; }
        public string CaptureJson() => JsonUtility.ToJson(InventorySaveData.FromInventory(_getter?.Invoke()));
        public void RestoreJson(string json) => _setter?.Invoke(JsonUtility.FromJson<InventorySaveData>(json).ToInventory(_capacity, _resolver));
    }
}
