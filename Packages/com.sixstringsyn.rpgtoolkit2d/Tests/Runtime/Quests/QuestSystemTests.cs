using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Quests
{
    public sealed class QuestSystemTests
    {
        [Test]
        public void Quest_CanStartProgressCompleteAndTurnIn()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            var objective = new QuestObjectiveDefinition();
            Set(objective, "_type", QuestObjectiveType.CustomEvent);
            Set(objective, "_targetId", "found_relic");
            AddToList(quest, "_objectives", objective);
            var reward = new QuestRewardDefinition();
            Set(reward, "_type", QuestRewardType.Currency);
            Set(reward, "_quantity", 25);
            Set(reward, "_currencyId", "gold");
            AddToList(quest, "_rewards", reward);
            var tracker = new QuestTracker();
            var instance = tracker.StartQuest(quest);
            Assert.AreEqual(QuestState.Active, instance.State);
            Assert.IsTrue(tracker.Advance(quest, QuestObjectiveType.CustomEvent, "found_relic"));
            Assert.AreEqual(QuestState.Completed, instance.State);
            var receiver = new QuestRewardReceiver();
            Assert.IsTrue(tracker.TurnIn(quest, receiver));
            Assert.AreEqual(QuestState.TurnedIn, instance.State);
            Assert.AreEqual(25, receiver.Currencies["gold"]);
            Object.DestroyImmediate(quest);
        }

        [Test]
        public void QuestSaveData_RestoresObjectiveProgress()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            var objective = new QuestObjectiveDefinition();
            Set(objective, "_type", QuestObjectiveType.KillTarget);
            Set(objective, "_targetId", "slime");
            Set(objective, "_requiredAmount", 2);
            AddToList(quest, "_objectives", objective);
            var tracker = new QuestTracker();
            tracker.StartQuest(quest);
            tracker.Advance(quest, QuestObjectiveType.KillTarget, "slime");
            var save = tracker.ToSaveData();
            var restored = new QuestTracker();
            restored.Load(save, new[] { quest });
            Assert.AreEqual(1, restored.GetQuest(quest).Objectives[0].Progress);
            Object.DestroyImmediate(quest);
        }

        [Test]
        public void QuestObjectiveTemplatesSupportDesignerAuthoredObjectives()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.AddObjective(QuestObjectiveDefinition.Create(QuestObjectiveType.KillTarget, "Kill 10 Slimes", "slime", 10));
            quest.AddObjective(QuestObjectiveDefinition.Create(QuestObjectiveType.EscortNPC, "Escort the merchant", "merchant"));
            quest.AddObjective(QuestObjectiveDefinition.Create(QuestObjectiveType.CraftItem, "Craft an iron sword", "iron_sword"));

            Assert.That(quest.Objectives[0].RequiredAmount, Is.EqualTo(10));
            Assert.That(quest.Objectives[1].Type, Is.EqualTo(QuestObjectiveType.EscortNPC));
            Assert.That(quest.Objectives[2].Matches(QuestObjectiveType.CraftItem, "iron_sword", null), Is.True);
            Object.DestroyImmediate(quest);
        }
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        private static void AddToList<T>(QuestDefinition quest, string field, T value) => ((System.Collections.Generic.List<T>)typeof(QuestDefinition).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quest)).Add(value);
    }
}
