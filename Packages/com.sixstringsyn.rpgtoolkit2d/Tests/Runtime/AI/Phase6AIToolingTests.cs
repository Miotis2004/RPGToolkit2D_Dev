using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.AI;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.AI
{
    public sealed class Phase6AIToolingTests
    {
        [Test] public void BehaviorTreeExecutorRunsConditionAndActionSequence()
        {
            var tree = ScriptableObject.CreateInstance<BehaviorTreeDefinition>();
            Set(tree, "_rootNodeId", "root");
            Set(tree, "_nodes", new List<BehaviorTreeNode> { new BehaviorTreeNode { nodeId = "root", kind = BehaviorTreeNodeKind.Sequence, children = new List<string> { "canAttack", "attack" } }, new BehaviorTreeNode { nodeId = "canAttack", kind = BehaviorTreeNodeKind.Condition, conditionKey = "target.visible", expectedBoolValue = true }, new BehaviorTreeNode { nodeId = "attack", kind = BehaviorTreeNodeKind.Action, actionId = "combat.attack" } });
            var blackboard = new BehaviorTreeBlackboard(); blackboard.Set("target.visible", true);
            var executor = new BehaviorTreeExecutor(); executor.RegisterAction("combat.attack", b => BehaviorTreeStatus.Success);
            var result = executor.Tick(tree, blackboard);
            Assert.That(result.Status, Is.EqualTo(BehaviorTreeStatus.Success));
            Assert.That(result.VisitedNodeIds, Is.EqualTo(new[] { "root", "canAttack", "attack" }));
            Object.DestroyImmediate(tree);
        }

        [Test] public void BehaviorTreeValidationFindsMissingChildren()
        {
            var tree = ScriptableObject.CreateInstance<BehaviorTreeDefinition>(); Set(tree, "_rootNodeId", "root"); Set(tree, "_nodes", new List<BehaviorTreeNode> { new BehaviorTreeNode { nodeId = "root", kind = BehaviorTreeNodeKind.Selector, children = new List<string> { "missing" } } });
            Assert.That(tree.ValidateTree().IsValid, Is.False); Object.DestroyImmediate(tree);
        }
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
