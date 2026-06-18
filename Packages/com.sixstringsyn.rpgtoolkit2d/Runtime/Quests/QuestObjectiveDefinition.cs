using System;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Quests
{
    public enum QuestObjectiveType { CollectItem, TalkToNPC, ReachLocation, KillTarget, EscortNPC, CraftItem, CustomEvent, CustomScript }

    [Serializable]
    public sealed class QuestObjectiveDefinition
    {
        [SerializeField] private string _id = Guid.NewGuid().ToString("N");
        [SerializeField] private string _description;
        [SerializeField] private QuestObjectiveType _type;
        [SerializeField] private ItemDefinition _item;
        [SerializeField] private string _targetId;
        [SerializeField] private int _requiredAmount = 1;
        [SerializeField] private bool _optional;

        public string Id => string.IsNullOrWhiteSpace(_id) ? _description : _id;
        public string Description => _description ?? string.Empty;
        public QuestObjectiveType Type => _type;
        public ItemDefinition Item => _item;
        public string TargetId => _targetId ?? string.Empty;
        public int RequiredAmount => Mathf.Max(1, _requiredAmount);
        public bool Optional => _optional;

        public static QuestObjectiveDefinition Create(QuestObjectiveType type, string description, string targetId = null, int requiredAmount = 1, bool optional = false)
        {
            return new QuestObjectiveDefinition { _type = type, _description = description, _targetId = targetId, _requiredAmount = Mathf.Max(1, requiredAmount), _optional = optional };
        }

        public static QuestObjectiveDefinition Collect(ItemDefinition item, int requiredAmount, string description = null, bool optional = false)
        {
            return new QuestObjectiveDefinition { _type = QuestObjectiveType.CollectItem, _description = description, _item = item, _requiredAmount = Mathf.Max(1, requiredAmount), _optional = optional };
        }

        public bool Matches(QuestObjectiveType type, string targetId, ItemDefinition item)
        {
            if (_type != type) return false;
            if (_type == QuestObjectiveType.CollectItem) return _item == item && _item != null;
            return string.IsNullOrWhiteSpace(_targetId) || string.Equals(_targetId, targetId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
