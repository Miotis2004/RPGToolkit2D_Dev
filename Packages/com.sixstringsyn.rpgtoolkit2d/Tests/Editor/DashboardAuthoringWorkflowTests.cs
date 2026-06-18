using System.IO;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor.Dashboard;
using SixStringSyn.RPGToolkit2D.Editor.Foundation;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor
{
    public sealed class DashboardAuthoringWorkflowTests
    {
        private const string TestFolder = "Assets/RPGToolkitDashboardTests";

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(TestFolder)) AssetDatabase.DeleteAsset(TestFolder);
        }

        [Test]
        public void DashboardWindowOpensWithoutExceptions()
        {
            var window = EditorWindow.GetWindow<RPGToolkitDashboardWindow>();
            Assert.That(window, Is.Not.Null);
            window.Close();
        }


        [Test]
        public void GameBuilderWindowExposesPhase7Tabs()
        {
            var window = EditorWindow.GetWindow<RPGToolkitGameBuilderWindow>();
            Assert.That(window, Is.Not.Null);

            foreach (var tab in RPGToolkitGameBuilderWindow.RequiredPhase7Tabs)
            {
                Assert.That(tab, Is.Not.Empty);
            }

            Assert.That(RPGToolkitGameBuilderWindow.RequiredPhase7Tabs, Does.Contain("Characters"));
            Assert.That(RPGToolkitGameBuilderWindow.RequiredPhase7Tabs, Does.Contain("Save Data"));
            Assert.That(RPGToolkitGameBuilderWindow.RequiredPhase7Tabs, Does.Contain("Settings"));
            window.Close();
        }

        [Test]
        public void AuthoringSectionsCoverPhase11ContentTypes()
        {
            var titles = string.Join(",", System.Linq.Enumerable.Select(RPGToolkitAuthoringWorkflow.Sections, section => section.Title));
            Assert.That(titles, Does.Contain("Characters"));
            Assert.That(titles, Does.Contain("Items"));
            Assert.That(titles, Does.Contain("Quests"));
            Assert.That(titles, Does.Contain("Dialogue"));
            Assert.That(titles, Does.Contain("Abilities"));
            Assert.That(titles, Does.Contain("Vendors"));
            Assert.That(titles, Does.Contain("Loot Tables"));
            Assert.That(titles, Does.Contain("NPCs"));
        }

        [Test]
        public void CreationWizardCreatesValidCharacterAsset()
        {
            var characterSection = System.Linq.Enumerable.First(RPGToolkitAuthoringWorkflow.Sections, section => section.AssetType == typeof(CharacterDefinition));
            var asset = RPGToolkitAuthoringWorkflow.CreateAsset(characterSection, TestFolder) as CharacterDefinition;

            Assert.That(asset, Is.Not.Null);
            Assert.That(asset.Id.IsEmpty, Is.False);
            Assert.That(AssetDatabase.GetAssetPath(asset), Does.StartWith(TestFolder));
        }

        [Test]
        public void DatabaseBrowserFindsCreatedAssetsAndSearchesByName()
        {
            var itemSection = System.Linq.Enumerable.First(RPGToolkitAuthoringWorkflow.Sections, section => section.AssetType == typeof(ItemDefinition));
            var asset = RPGToolkitAuthoringWorkflow.CreateAsset(itemSection, TestFolder) as ItemDefinition;
            asset.name = "Dashboard Search Potion";
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            var entries = RPGToolkitAuthoringWorkflow.FindAssets(itemSection, "Search Potion");

            Assert.That(entries, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(System.Linq.Enumerable.Any(entries, entry => entry.Asset == asset), Is.True);
        }

        [Test]
        public void ProjectSetupValidatorReportsAuthoringFolderGuidance()
        {
            if (Directory.Exists(TestFolder)) AssetDatabase.DeleteAsset(TestFolder);
            var results = RPGToolkitAuthoringWorkflow.ValidateProjectSetup();
            Assert.That(results, Is.Not.Empty);
            Assert.That(System.Linq.Enumerable.Any(results, result => result.RuleName == "Authoring asset folder"), Is.True);
        }
    }
}
