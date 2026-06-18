using System;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Quests
{
    public enum QuestObjectiveType { CollectItem, TalkToNPC, ReachLocation, KillTarget, CustomEvent, CustomScript }

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

        public bool Matches(QuestObjectiveType type, string targetId, ItemDefinition item)
        {
            if (_type != type) return false;
            if (_type == QuestObjectiveType.CollectItem) return _item == item && _item != null;
            return string.IsNullOrWhiteSpace(_targetId) || string.Equals(_targetId, targetId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
