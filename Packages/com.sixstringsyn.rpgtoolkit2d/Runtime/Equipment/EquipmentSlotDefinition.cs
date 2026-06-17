using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Equipment
{
    [CreateAssetMenu(fileName = "NewEquipmentSlot", menuName = "RPG Toolkit/Equipment/Equipment Slot")]
    public sealed class EquipmentSlotDefinition : ScriptableObject
    {
        [SerializeField] private string _slotId = "slot";
        [SerializeField] private string _displayName = "Slot";
        public string SlotId => string.IsNullOrWhiteSpace(_slotId) ? name : _slotId.Trim();
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
    }
}
