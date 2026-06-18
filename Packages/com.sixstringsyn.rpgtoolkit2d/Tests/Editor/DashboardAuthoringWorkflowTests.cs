using System.IO;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor.Dashboard;
using SixStringSyn.RPGToolkit2D.Editor.Foundation;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using SixStringSyn.RPGToolkit2D.Editor;
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
        public void AuthoringSectionsExposeCapabilityStatusMetadata()
        {
            foreach (var section in RPGToolkitAuthoringWorkflow.Sections)
            {
                Assert.That(section.Capability, Is.Not.Null, section.Title);
                Assert.That(section.Capability.ContentTitle, Is.EqualTo(section.Title), section.Title);
                Assert.That(section.Capability.AssetType, Is.EqualTo(section.AssetType), section.Title);
                Assert.That(section.Capability.AssetCreationStatus, Is.EqualTo(RPGToolkitDashboardCapabilityStatus.Complete), section.Title);
                Assert.That(section.Capability.Notes, Is.Not.Empty, section.Title);
            }
        }

        [Test]
        public void DashboardCapabilitiesCoverKnownCompletePartialAndMissingStates()
        {
            var statuses = System.Linq.Enumerable.SelectMany(RPGToolkitAuthoringWorkflow.Sections, section => new[]
            {
                section.Capability.AssetCreationStatus,
                section.Capability.FocusedEditorStatus,
                section.Capability.ValidationStatus,
                section.Capability.DocumentationStatus,
                section.Capability.RuntimeIntegrationStatus
            });

            Assert.That(statuses, Does.Contain(RPGToolkitDashboardCapabilityStatus.Complete));
            Assert.That(statuses, Does.Contain(RPGToolkitDashboardCapabilityStatus.Partial));
            Assert.That(statuses, Does.Contain(RPGToolkitDashboardCapabilityStatus.Missing));
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
        public void AuthoringSectionsCoverPhase8MapWorkflowTypes()
        {
            var titles = string.Join(",", System.Linq.Enumerable.Select(RPGToolkitAuthoringWorkflow.Sections, section => section.Title));
            Assert.That(titles, Does.Contain("Maps"));
            Assert.That(titles, Does.Contain("Tilesets"));
            Assert.That(titles, Does.Contain("Sprite Sheets"));
            Assert.That(titles, Does.Contain("Sprite Sheet Profiles"));
        }

        [Test]
        public void ProjectWideMapValidationAggregatesBrokenAssetsAndDuplicateIds()
        {
            var mapSection = System.Linq.Enumerable.First(RPGToolkitAuthoringWorkflow.Sections, section => section.AssetType == typeof(RPGMapDefinition));
            var sheetSection = System.Linq.Enumerable.First(RPGToolkitAuthoringWorkflow.Sections, section => section.AssetType == typeof(RPGSpriteSheetAsset));
            var map = RPGToolkitAuthoringWorkflow.CreateAsset(mapSection, TestFolder) as RPGMapDefinition;
            var sheet = RPGToolkitAuthoringWorkflow.CreateAsset(sheetSection, TestFolder) as RPGSpriteSheetAsset;
            sheet.SetId(map.Id);
            EditorUtility.SetDirty(sheet);
            AssetDatabase.SaveAssets();

            var report = RPGMapProjectValidationService.ValidateAllMapContent();

            Assert.That(report.Entries, Has.Count.GreaterThanOrEqualTo(2));
            Assert.That(System.Linq.Enumerable.Any(report.Entries, entry => entry.Message.Code == "RPG_MAP_MISSING_TILESET"), Is.True);
            Assert.That(System.Linq.Enumerable.Any(report.Entries, entry => entry.Message.Code == "RPG_MAP_ASSET_DUPLICATE_ID"), Is.True);
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
