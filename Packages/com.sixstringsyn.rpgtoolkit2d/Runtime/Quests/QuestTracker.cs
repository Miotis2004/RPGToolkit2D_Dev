using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using SixStringSyn.RPGToolkit2D.Runtime.Items;

namespace SixStringSyn.RPGToolkit2D.Runtime.Quests
{
    public sealed class QuestTracker
    {
        private readonly Dictionary<string, QuestInstance> _quests = new Dictionary<string, QuestInstance>();
        private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        public event Action<QuestInstance> QuestChanged;
        public IReadOnlyCollection<QuestInstance> Quests => _quests.Values;
        public QuestInstance GetQuest(QuestDefinition definition) => definition != null && _quests.TryGetValue(definition.Id.Value, out var quest) ? quest : null;
        public bool CanStart(QuestDefinition definition) => definition != null && definition.Conditions.All(IsConditionMet);
        public QuestInstance StartQuest(QuestDefinition definition) { if (!CanStart(definition)) return null; if (!_quests.TryGetValue(definition.Id.Value, out var quest)) _quests[definition.Id.Value] = quest = new QuestInstance(definition); if (quest.Start()) QuestChanged?.Invoke(quest); return quest; }
        public bool Advance(QuestDefinition quest, QuestObjectiveType type, string targetId = null, ItemDefinition item = null, int amount = 1) { var instance = GetQuest(quest); var changed = instance != null && instance.Advance(type, targetId, item, amount); if (changed) QuestChanged?.Invoke(instance); return changed; }
        public bool AdvanceAll(QuestObjectiveType type, string targetId = null, ItemDefinition item = null, int amount = 1) { var changed = false; foreach (var instance in _quests.Values.ToArray()) if (instance.Advance(type, targetId, item, amount)) { changed = true; QuestChanged?.Invoke(instance); } return changed; }
        public bool TurnIn(QuestDefinition quest, IQuestRewardReceiver receiver = null) { var instance = GetQuest(quest); var changed = instance != null && instance.TurnIn(receiver); if (changed) QuestChanged?.Invoke(instance); return changed; }
        public void SetFlag(string flag, bool value) => _flags[flag] = value;
        public bool GetFlag(string flag) => !string.IsNullOrWhiteSpace(flag) && _flags.TryGetValue(flag, out var value) && value;
        public void Bind(DialogueRunner runner) { if (runner != null) runner.CommandExecuted += ExecuteDialogueCommand; }
        public void ExecuteDialogueCommand(DialogueCommand command)
        {
            if (command == null) return;
            if (string.Equals(command.Name, "quest_event", StringComparison.OrdinalIgnoreCase)) AdvanceAll(QuestObjectiveType.CustomEvent, command.Argument);
        }
        public QuestJournalSaveData ToSaveData() => new QuestJournalSaveData { quests = _quests.Values.Select(q => q.ToSaveData()).ToList() };
        public void Load(QuestJournalSaveData data, IEnumerable<QuestDefinition> definitions) { _quests.Clear(); if (data == null || definitions == null) return; var byId = definitions.Where(d => d != null).ToDictionary(d => d.Id.Value); foreach (var record in data.quests) if (byId.TryGetValue(record.questId, out var def)) { var instance = new QuestInstance(def); instance.Load(record); _quests[record.questId] = instance; } }
        private bool IsConditionMet(QuestCondition condition)
        {
            if (condition == null) return true;
            if (condition.Type == QuestConditionType.CustomFlag) return GetFlag(condition.Flag) == condition.ExpectedValue;
            var state = GetQuest(condition.Quest)?.State ?? QuestState.Inactive;
            return condition.Type == QuestConditionType.QuestInactive ? state == QuestState.Inactive : condition.Type == QuestConditionType.QuestActive ? state == QuestState.Active : condition.Type == QuestConditionType.QuestCompleted ? state == QuestState.Completed : condition.Type == QuestConditionType.QuestTurnedIn ? state == QuestState.TurnedIn : state == QuestState.Failed;
        }
    }
}
