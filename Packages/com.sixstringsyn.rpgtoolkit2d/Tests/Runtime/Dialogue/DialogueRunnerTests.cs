using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Dialogue
{
    public sealed class DialogueRunnerTests
    {
        [Test]
        public void RunnerBranchesThroughAvailableChoiceAndFiresCommands()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            var start = dialogue.AddNode(DialogueNodeType.Entry, "NPC", "Choose.");
            var locked = dialogue.AddNode(DialogueNodeType.Line, "NPC", "Locked.");
            var unlocked = dialogue.AddNode(DialogueNodeType.Line, "NPC", "Unlocked.");
            start.AddChoice(new DialogueChoice("Locked", locked.NodeId));
            var choice = new DialogueChoice("Unlocked", unlocked.NodeId);
            choice.AddCondition(new DialogueCondition("has_key", DialogueConditionOperator.Equals, "true"));
            choice.AddCommand(new DialogueCommand("take_key"));
            start.AddChoice(choice);
            var context = new DictionaryDialogueContext();
            context.Set("has_key", "true");
            var runner = new DialogueRunner();
            var fired = false;
            runner.CommandExecuted += command => fired = command.Name == "take_key";

            Assert.That(runner.Start(dialogue, context), Is.True);
            Assert.That(runner.AvailableChoices.Count, Is.EqualTo(2));
            Assert.That(runner.SelectChoice(1), Is.True);
            Assert.That(runner.CurrentNode, Is.EqualTo(unlocked));
            Assert.That(fired, Is.True);
            Object.DestroyImmediate(dialogue);
        }

        [Test]
        public void DialogueNodeStoresPresentationMetadataForAuthoring()
        {
            var node = new DialogueNode(DialogueNodeType.QuestUpdate, "Quest updated.");
            node.SetPresentation("dialogue.quest.updated", speakerAnimation: "Celebrate");

            Assert.That(node.NodeType, Is.EqualTo(DialogueNodeType.QuestUpdate));
            Assert.That(node.LocalizationKey, Is.EqualTo("dialogue.quest.updated"));
            Assert.That(node.SpeakerAnimation, Is.EqualTo("Celebrate"));
            Assert.That(DialogueCommand.QuestEvent("found_relic").Name, Is.EqualTo("quest_event"));
            Assert.That(DialogueCommand.QuestEvent("found_relic").Argument, Is.EqualTo("found_relic"));
        }
    }
}
