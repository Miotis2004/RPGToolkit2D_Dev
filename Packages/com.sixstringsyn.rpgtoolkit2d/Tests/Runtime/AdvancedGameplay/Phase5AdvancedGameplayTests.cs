using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Factions;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Loot;
using SixStringSyn.RPGToolkit2D.Runtime.SkillTrees;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.AdvancedGameplay
{
    public sealed class Phase5AdvancedGameplayTests
    {
        [Test] public void ReputationClampsAndEvaluatesConditions()
        {
            var faction = ScriptableObject.CreateInstance<FactionDefinition>(); faction.SetId(new RPGId("faction.guild"));
            var reputation = new ReputationSystem(); reputation.Add(faction, 250, "quest");
            Assert.That(reputation.Get(faction), Is.EqualTo(100));
            Assert.That(reputation.Meets(new ReputationCondition { faction = faction, minimumValue = 50 }), Is.True);
            Object.DestroyImmediate(faction);
        }
        [Test] public void SkillTreeRequiresPrerequisitesAndSpendsPoints()
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            Set(tree, "_nodes", new List<SkillTreeNode> { new SkillTreeNode { nodeId = "slash", pointCost = 1 }, new SkillTreeNode { nodeId = "cleave", pointCost = 2, prerequisites = new List<string> { "slash" } } });
            var progression = new SkillTreeProgression(); var points = 3;
            Assert.That(progression.Unlock(tree, "cleave", ref points, out _), Is.False);
            Assert.That(progression.Unlock(tree, "slash", ref points, out _), Is.True);
            Assert.That(progression.Unlock(tree, "cleave", ref points, out _), Is.True); Assert.That(points, Is.EqualTo(0));
            Object.DestroyImmediate(tree);
        }
        [Test] public void NestedLootTablesCanBeSimulatedDeterministically()
        {
            var item = Item("item.gold"); var child = ScriptableObject.CreateInstance<LootTableDefinition>(); var parent = ScriptableObject.CreateInstance<LootTableDefinition>();
            Set(child, "_entries", new List<LootEntry> { new LootEntry { item = item, minQuantity = 2, maxQuantity = 2, weight = 1 } }); Set(parent, "_entries", new List<LootEntry> { new LootEntry { nestedTable = child, weight = 1 } });
            var result = LootRoller.Simulate(parent, 3, 99);
            Assert.That(result.drops.Count, Is.EqualTo(3)); Assert.That(result.drops[0].Quantity, Is.EqualTo(2)); Assert.That(parent.ValidateTable().IsValid, Is.True);
            Object.DestroyImmediate(item); Object.DestroyImmediate(child); Object.DestroyImmediate(parent);
        }
        private static ItemDefinition Item(string id) { var i = ScriptableObject.CreateInstance<ItemDefinition>(); i.SetId(new RPGId(id)); return i; }
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
