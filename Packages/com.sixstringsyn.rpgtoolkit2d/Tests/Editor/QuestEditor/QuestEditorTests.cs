using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor.QuestEditor;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor.QuestEditor
{
    public sealed class QuestEditorTests
    {
        [Test]
        public void QuestEditorWindow_Opens()
        {
            var window = EditorWindow.GetWindow<QuestEditorWindow>();
            Assert.IsNotNull(window);
            window.Close();
        }

        [Test]
        public void QuestValidation_ReportsMissingObjectives()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            var result = quest.ValidateQuest();
            Assert.IsFalse(result.IsValid);
            Object.DestroyImmediate(quest);
        }
    }
}
