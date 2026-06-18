using System;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Quests
{
    public enum QuestRewardType { Item, Experience, Currency, CustomAction }

    [Serializable]
    public sealed class QuestRewardDefinition
    {
        [SerializeField] private QuestRewardType _type;
        [SerializeField] private ItemDefinition _item;
        [SerializeField] private int _quantity = 1;
        [SerializeField] private string _currencyId = "gold";
        [SerializeField] private string _customAction;

        public QuestRewardType Type => _type;
        public ItemDefinition Item => _item;
        public int Quantity => Mathf.Max(1, _quantity);
        public string CurrencyId => _currencyId ?? string.Empty;
        public string CustomAction => _customAction ?? string.Empty;
    }
}
