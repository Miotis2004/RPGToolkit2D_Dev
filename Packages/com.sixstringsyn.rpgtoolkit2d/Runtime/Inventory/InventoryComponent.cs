using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Inventory
{
    public sealed class InventoryComponent : MonoBehaviour
    {
        [SerializeField] private int _capacity = 20;
        public InventoryContainer Inventory { get; private set; }
        private void Awake() => Inventory = new InventoryContainer(Mathf.Max(1, _capacity));
    }
}
