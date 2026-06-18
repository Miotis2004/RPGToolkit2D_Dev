using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using SixStringSyn.RPGToolkit2D.Runtime.Saving;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Performance
{
    public sealed class ReleasePerformanceSmokeTests
    {
        [Test]
        public void InventoryDatabaseDialogueQuestAndSaveStayWithinReleaseSmokeThresholds()
        {
            var stopwatch = Stopwatch.StartNew();
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.SetId(new RPGId("perf.item"));
            SetPrivateField(item, "_maximumStackSize", 99);
            var inventory = new InventoryContainer(250);
            for (var i = 0; i < 10000; i++) inventory.Add(item, 1);
            Assert.That(inventory.Count(item), Is.EqualTo(10000));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(750));

            stopwatch.Restart();
            var definitions = new List<ItemDefinition>();
            for (var i = 0; i < 1000; i++)
            {
                var definition = ScriptableObject.CreateInstance<ItemDefinition>();
                definition.SetId(new RPGId("perf.item." + i));
                definitions.Add(definition);
            }
            var database = new RPGDatabase<ItemDefinition>(definitions);
            Assert.That(database.Validate().IsValid, Is.True);
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500));

            stopwatch.Restart();
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            var previous = dialogue.AddNode(DialogueNodeType.Entry, "Guide", "0");
            for (var i = 1; i < 250; i++)
            {
                var node = dialogue.AddNode(DialogueNodeType.Line, "Guide", i.ToString());
                previous.SetNext(node.NodeId);
                previous = node;
            }
            var runner = new DialogueRunner();
            Assert.That(runner.Start(dialogue), Is.True);
            var traversed = 1;
            while (runner.Continue()) traversed++;
            Assert.That(traversed, Is.EqualTo(250));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500));

            stopwatch.Restart();
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.SetId(new RPGId("perf.quest"));
            var objective = new QuestObjectiveDefinition();
            SetPrivateField(objective, "_type", QuestObjectiveType.CustomEvent);
            SetPrivateField(objective, "_targetId", "tick");
            SetPrivateField(objective, "_requiredAmount", 1000);
            var objectives = (List<QuestObjectiveDefinition>)typeof(QuestDefinition).GetField("_objectives", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quest);
            objectives.Add(objective);
            var tracker = new QuestTracker();
            tracker.StartQuest(quest);
            for (var i = 0; i < 1000; i++) tracker.AdvanceAll(QuestObjectiveType.CustomEvent, "tick");
            Assert.That(tracker.GetQuest(quest).State, Is.EqualTo(QuestState.Completed));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500));

            stopwatch.Restart();
            var saveService = new SaveGameService();
            var data = saveService.Capture(123d, "PerfScene");
            data.SetSystemJson("inventory", JsonUtility.ToJson(new InventorySaveData()));
            var json = saveService.ToJson(data, false);
            Assert.That(saveService.FromJson(json), Is.Not.Null);
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(250));

            Object.DestroyImmediate(item);
            Object.DestroyImmediate(dialogue);
            Object.DestroyImmediate(quest);
            foreach (var definition in definitions) Object.DestroyImmediate(definition);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
