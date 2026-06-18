using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor.DialogueGraph;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor.DialogueGraph
{
    public sealed class DialogueGraphEditorTests
    {
        [Test]
        public void DialogueGraphValidationReportsMissingChoiceTargets()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            var start = dialogue.AddNode(DialogueNodeType.Entry, "NPC", "Hello");
            start.AddChoice(new DialogueChoice("Missing", "missing-node"));

            var result = dialogue.ValidateGraph();

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Messages[0].Code, Is.EqualTo("DIALOGUE_MISSING_CHOICE_TARGET"));
            Object.DestroyImmediate(dialogue);
        }

        [Test]
        public void DialogueGraphValidationReportsUnreachableNodes()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            var start = dialogue.AddNode(DialogueNodeType.Entry, "NPC", "Hello");
            var reachable = dialogue.AddNode(DialogueNodeType.Line, "NPC", "Reachable");
            dialogue.AddNode(DialogueNodeType.Line, "NPC", "Unreachable");
            start.SetNext(reachable.NodeId);

            var result = dialogue.ValidateGraph();

            Assert.That(result.Messages, Has.Some.Matches<RPGValidationMessage>(message => message.Code == "DIALOGUE_UNREACHABLE_NODE"));
            Object.DestroyImmediate(dialogue);
        }

        [Test]
        public void DialogueGraphSearchIncludesCommandsAndChoiceText()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            var start = dialogue.AddNode(DialogueNodeType.Entry, "NPC", "Hello");
            start.AddChoice(new DialogueChoice("Ask about the ruins", start.NodeId));
            start.AddCommand(DialogueCommand.SetVariable("world.ruins_intro", "seen"));

            Assert.That(DialogueGraphEditorWindow.SearchNodes(dialogue, "ruins_intro"), Has.Count.EqualTo(1));
            Assert.That(DialogueGraphEditorWindow.SearchNodes(dialogue, "ruins"), Has.Count.EqualTo(1));
            Object.DestroyImmediate(dialogue);
        }
    }
}
