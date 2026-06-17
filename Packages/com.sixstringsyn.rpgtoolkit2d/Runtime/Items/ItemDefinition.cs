using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Items
{
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum ItemType { Generic, Consumable, Weapon, Armor, QuestItem }

    [CreateAssetMenu(fileName = "NewItemDefinition", menuName = "RPG Toolkit/Item Definition")]
    public sealed class ItemDefinition : RPGObject
    {
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _maximumStackSize = 1;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;
        [SerializeField] private ItemType _itemType = ItemType.Generic;
        [SerializeField] private List<string> _allowedEquipmentSlots = new List<string>();
        [SerializeField] private ItemUseAction _useAction;

        public Sprite Icon => _icon;
        public int MaximumStackSize => Mathf.Max(1, _maximumStackSize);
        public bool IsStackable => MaximumStackSize > 1;
        public ItemRarity Rarity => _rarity;
        public ItemType ItemType => _itemType;
        public IReadOnlyList<string> AllowedEquipmentSlots => _allowedEquipmentSlots;
        public ItemUseAction UseAction => _useAction;

        protected override void OnValidate()
        {
            base.OnValidate();
            _maximumStackSize = Mathf.Max(1, _maximumStackSize);
        }
    }
}
