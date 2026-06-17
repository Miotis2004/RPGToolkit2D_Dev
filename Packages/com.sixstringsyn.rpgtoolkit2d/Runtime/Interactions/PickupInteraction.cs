using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Pickups;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    [RequireComponent(typeof(ItemPickup))]
    public sealed class PickupInteraction : InteractableBehaviour
    {
        [SerializeField] private ItemPickup _pickup;
        private void Reset() => _pickup = GetComponent<ItemPickup>();
        private void Awake() { if (_pickup == null) _pickup = GetComponent<ItemPickup>(); }
        public override bool CanInteract(GameObject interactor) => base.CanInteract(interactor) && _pickup != null && _pickup.Item != null;
        public override void Interact(GameObject interactor)
        {
            var inventory = interactor != null ? interactor.GetComponentInParent<InventoryComponent>() : null;
            if (inventory != null) _pickup.Collect(inventory);
        }
    }
}
