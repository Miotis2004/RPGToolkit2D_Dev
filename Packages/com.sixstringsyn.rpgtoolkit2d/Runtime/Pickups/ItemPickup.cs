using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Pickups
{
    public sealed class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField] private int _quantity = 1;
        [SerializeField] private bool _destroyWhenCollected = true;
        public ItemDefinition Item => _item;
        public int Quantity => Mathf.Max(1, _quantity);

        public bool Collect(InventoryContainer inventory)
        {
            if (inventory == null || _item == null) return false;
            var added = inventory.Add(_item, Quantity);
            if (added <= 0) return false;
            _quantity -= added;
            if (_destroyWhenCollected && _quantity <= 0) Destroy(gameObject);
            return true;
        }

        public bool Collect(InventoryComponent inventory) => inventory != null && Collect(inventory.Inventory);
    }
}
