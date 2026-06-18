using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor.QuestEditor;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor.QuestEditor
{
    public sealed class QuestEditorTests
    {
        private const string TestFolder = "Assets/RPGToolkit2DQuestEditorTests";

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.CreateFolder("Assets", "RPGToolkit2DQuestEditorTests");
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestFolder);
        }
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


        [Test]
        public void QuestEditorWorkflow_CreatesQuestAsset()
        {
            var quest = QuestEditorWindow.CreateQuestAsset(TestFolder, "CreatedQuest.asset");
            Assert.IsNotNull(quest);
            Assert.IsTrue(File.Exists(AssetDatabase.GetAssetPath(quest)));
        }

        [Test]
        public void QuestEditorWorkflow_AddsObjectiveAndReward()
        {
            var quest = QuestEditorWindow.CreateQuestAsset(TestFolder, "EditedQuest.asset");
            QuestEditorWindow.AddObjective(quest, QuestObjectiveType.CustomEvent, "Find the relic", "found_relic");
            QuestEditorWindow.AddReward(quest, QuestRewardType.Experience, quantity: 25);

            Assert.That(quest.Objectives.Count, Is.EqualTo(1));
            Assert.That(quest.Rewards.Count, Is.EqualTo(1));
            Assert.That(quest.ValidateQuest().IsValid, Is.True);
        }

        [Test]
        public void QuestEditorWorkflow_ValidationReportsRewardErrors()
        {
            var quest = QuestEditorWindow.CreateQuestAsset(TestFolder, "InvalidRewardQuest.asset");
            QuestEditorWindow.AddObjective(quest, QuestObjectiveType.CustomEvent, "Find the relic", "found_relic");
            QuestEditorWindow.AddReward(quest, QuestRewardType.Item);

            var result = QuestEditorWindow.ValidateQuestWorkflow(quest);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Messages.Count(message => message.Code == "QUEST_REWARD_ITEM_MISSING"), Is.EqualTo(1));
        }
    }
}
