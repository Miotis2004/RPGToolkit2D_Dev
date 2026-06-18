using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Events;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.World;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Events
{
    public sealed class EventSystemTests
    {
        [Test]
        public void EventRunnerExecutesVariableAndInventoryCommands()
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.SetId(new RPGId("item.key"));
            item.GetType().GetField("_maximumStackSize", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(item, 10);

            var evt = ScriptableObject.CreateInstance<RPGEventDefinition>();
            evt.SetId(new RPGId("event.chest"));
            Set(evt, "_commands", new List<RPGEventCommand>
            {
                new RPGEventCommand { type = RPGEventCommandType.ChangeVariable, targetId = "ChestOpened", variableOperation = RPGEventVariableOperation.SetFlag, argument = "true" },
                new RPGEventCommand { type = RPGEventCommandType.GiveItem, item = item, amount = 2 }
            });

            var state = new WorldState();
            var inventory = new InventoryContainer(4);
            var runner = new RPGEventRunner(new RPGEventContext { WorldState = state, Inventory = inventory });

            var result = runner.Run(evt);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ExecutedCommands, Is.EqualTo(2));
            Assert.That(state.GetFlag("ChestOpened"), Is.True);
            Assert.That(inventory.Count(item), Is.EqualTo(2));
            Object.DestroyImmediate(evt);
            Object.DestroyImmediate(item);
        }

        [Test]
        public void RunOnceEventMarksCompletionInWorldState()
        {
            var evt = ScriptableObject.CreateInstance<RPGEventDefinition>();
            evt.SetId(new RPGId("event.once"));
            Set(evt, "_runOnce", true);
            Set(evt, "_commands", new List<RPGEventCommand> { new RPGEventCommand { type = RPGEventCommandType.ChangeVariable, targetId = "Counter", variableOperation = RPGEventVariableOperation.AddCounter, amount = 1 } });
            var state = new WorldState();
            var runner = new RPGEventRunner(new RPGEventContext { WorldState = state });

            Assert.That(runner.Run(evt).ExecutedCommands, Is.EqualTo(1));
            Assert.That(runner.Run(evt).ExecutedCommands, Is.EqualTo(0));
            Assert.That(state.GetCounter("Counter"), Is.EqualTo(1));
            Assert.That(state.GetFlag("event.event.once.completed"), Is.True);
            Object.DestroyImmediate(evt);
        }

        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
