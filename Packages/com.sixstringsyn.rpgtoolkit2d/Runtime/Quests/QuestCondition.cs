using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Quests
{
    public enum QuestConditionType { QuestInactive, QuestActive, QuestCompleted, QuestTurnedIn, QuestFailed, CustomFlag }

    [Serializable]
    public sealed class QuestCondition
    {
        [SerializeField] private QuestConditionType _type;
        [SerializeField] private QuestDefinition _quest;
        [SerializeField] private string _flag;
        [SerializeField] private bool _expectedValue = true;

        public QuestConditionType Type => _type;
        public QuestDefinition Quest => _quest;
        public string Flag => _flag ?? string.Empty;
        public bool ExpectedValue => _expectedValue;
    }
}
