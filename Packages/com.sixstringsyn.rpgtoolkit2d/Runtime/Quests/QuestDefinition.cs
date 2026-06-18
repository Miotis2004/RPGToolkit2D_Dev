using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Quests
{
    [CreateAssetMenu(fileName = "NewQuestDefinition", menuName = "RPG Toolkit/Quest Definition")]
    public sealed class QuestDefinition : RPGObject
    {
        [SerializeField] private List<QuestCondition> _conditions = new List<QuestCondition>();
        [SerializeField] private List<QuestObjectiveDefinition> _objectives = new List<QuestObjectiveDefinition>();
        [SerializeField] private List<QuestRewardDefinition> _rewards = new List<QuestRewardDefinition>();
        [SerializeField] private bool _autoTurnIn;

        public IReadOnlyList<QuestCondition> Conditions => _conditions;
        public IReadOnlyList<QuestObjectiveDefinition> Objectives => _objectives;
        public IReadOnlyList<QuestRewardDefinition> Rewards => _rewards;
        public bool AutoTurnIn => _autoTurnIn;

        public RPGValidationResult ValidateQuest()
        {
            var result = new RPGValidationResult();
            if (_objectives.Count == 0) result.AddError("QUEST_NO_OBJECTIVES", $"Quest '{DisplayName}' has no objectives.", Id);
            foreach (var objective in _objectives)
            {
                if (objective.Type == QuestObjectiveType.CollectItem && objective.Item == null) result.AddError("QUEST_OBJECTIVE_ITEM_MISSING", $"Collect objective '{objective.Description}' is missing an item.", Id);
                if (objective.Type != QuestObjectiveType.CollectItem && objective.Type != QuestObjectiveType.CustomScript && string.IsNullOrWhiteSpace(objective.TargetId)) result.AddWarning("QUEST_OBJECTIVE_TARGET_MISSING", $"Objective '{objective.Description}' has no target id and may be unreachable.", Id);
            }
            if (_rewards.Count == 0) result.AddWarning("QUEST_NO_REWARDS", $"Quest '{DisplayName}' has no rewards.", Id);
            foreach (var reward in _rewards.Where(r => r.Type == QuestRewardType.Item && r.Item == null)) result.AddError("QUEST_REWARD_ITEM_MISSING", $"Quest '{DisplayName}' has an item reward without an item.", Id);
            return result;
        }
    }
}
