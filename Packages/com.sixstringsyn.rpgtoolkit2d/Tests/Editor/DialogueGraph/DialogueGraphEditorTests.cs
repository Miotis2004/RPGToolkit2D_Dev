using NUnit.Framework;
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
    }
}
