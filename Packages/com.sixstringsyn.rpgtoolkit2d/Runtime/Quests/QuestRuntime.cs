using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Saving;

namespace SixStringSyn.RPGToolkit2D.Runtime.Quests
{
    public enum QuestState { Inactive, Active, Completed, Failed, TurnedIn }

    [Serializable]
    public sealed class QuestObjectiveSaveData { public string objectiveId; public int progress; public bool completed; }
    [Serializable]
    public sealed class QuestRecordSaveData { public string questId; public QuestState state; public List<QuestObjectiveSaveData> objectives = new List<QuestObjectiveSaveData>(); }
    [Serializable]
    public sealed class QuestJournalSaveData { public List<QuestRecordSaveData> quests = new List<QuestRecordSaveData>(); }

    public interface IQuestRewardReceiver
    {
        int GrantItem(ItemDefinition item, int quantity);
        void GrantExperience(int amount);
        void GrantCurrency(string currencyId, int amount);
        void ExecuteCustomReward(string actionId, QuestDefinition quest);
    }

    public sealed class QuestRewardReceiver : IQuestRewardReceiver
    {
        private readonly InventoryContainer _inventory;
        private readonly CharacterInstance _character;
        private readonly Dictionary<string, int> _currencies = new Dictionary<string, int>();
        public QuestRewardReceiver(InventoryContainer inventory = null, CharacterInstance character = null) { _inventory = inventory; _character = character; }
        public IReadOnlyDictionary<string, int> Currencies => _currencies;
        public int GrantItem(ItemDefinition item, int quantity) => _inventory == null || item == null ? 0 : _inventory.Add(item, quantity);
        public void GrantExperience(int amount) => _character?.AddExperience(amount);
        public void GrantCurrency(string currencyId, int amount) { if (amount <= 0) return; var id = string.IsNullOrWhiteSpace(currencyId) ? "gold" : currencyId; _currencies[id] = _currencies.TryGetValue(id, out var current) ? current + amount : amount; }
        public void ExecuteCustomReward(string actionId, QuestDefinition quest) { }
    }

    public sealed class QuestObjectiveProgress
    {
        public QuestObjectiveProgress(QuestObjectiveDefinition definition) { Definition = definition ?? throw new ArgumentNullException(nameof(definition)); }
        public QuestObjectiveDefinition Definition { get; }
        public int Progress { get; private set; }
        public bool IsCompleted { get; private set; }
        public void SetProgress(int value) { Progress = Math.Max(0, Math.Min(value, Definition.RequiredAmount)); IsCompleted = Progress >= Definition.RequiredAmount; }
        public bool Advance(int amount = 1) { if (IsCompleted) return false; var before = Progress; SetProgress(Progress + Math.Max(1, amount)); return Progress != before; }
    }

    public sealed class QuestInstance
    {
        private readonly List<QuestObjectiveProgress> _objectives;
        public QuestInstance(QuestDefinition definition) { Definition = definition ?? throw new ArgumentNullException(nameof(definition)); _objectives = definition.Objectives.Select(o => new QuestObjectiveProgress(o)).ToList(); }
        public QuestDefinition Definition { get; }
        public QuestState State { get; private set; }
        public IReadOnlyList<QuestObjectiveProgress> Objectives => _objectives;
        public bool Start() { if (State != QuestState.Inactive) return false; State = QuestState.Active; return true; }
        public bool Fail() { if (State != QuestState.Active) return false; State = QuestState.Failed; return true; }
        public bool TurnIn(IQuestRewardReceiver receiver = null) { if (State != QuestState.Completed) return false; foreach (var reward in Definition.Rewards) Grant(reward, receiver); State = QuestState.TurnedIn; return true; }
        public bool Advance(QuestObjectiveType type, string targetId = null, ItemDefinition item = null, int amount = 1) { if (State != QuestState.Active) return false; var changed = false; foreach (var objective in _objectives.Where(o => !o.IsCompleted && o.Definition.Matches(type, targetId, item))) changed |= objective.Advance(amount); if (changed && RequiredComplete) State = QuestState.Completed; return changed; }
        private bool RequiredComplete => _objectives.Where(o => !o.Definition.Optional).All(o => o.IsCompleted);
        private void Grant(QuestRewardDefinition reward, IQuestRewardReceiver receiver) { if (receiver == null) return; if (reward.Type == QuestRewardType.Item) receiver.GrantItem(reward.Item, reward.Quantity); else if (reward.Type == QuestRewardType.Experience) receiver.GrantExperience(reward.Quantity); else if (reward.Type == QuestRewardType.Currency) receiver.GrantCurrency(reward.CurrencyId, reward.Quantity); else receiver.ExecuteCustomReward(reward.CustomAction, Definition); }
        public QuestRecordSaveData ToSaveData() => new QuestRecordSaveData { questId = Definition.Id.Value, state = State, objectives = _objectives.Select(o => new QuestObjectiveSaveData { objectiveId = o.Definition.Id, progress = o.Progress, completed = o.IsCompleted }).ToList() };
        public void Load(QuestRecordSaveData data) { if (data == null) return; State = data.state; foreach (var saved in data.objectives) _objectives.FirstOrDefault(o => o.Definition.Id == saved.objectiveId)?.SetProgress(saved.progress); }
    }
}
