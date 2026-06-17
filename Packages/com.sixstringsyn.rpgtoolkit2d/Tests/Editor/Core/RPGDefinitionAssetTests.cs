using System.IO;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Abilities;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor.Core
{
    public sealed class RPGDefinitionAssetTests
    {
        private const string TestFolder = "Assets/RPGToolkit2DGeneratedTests";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestFolder);
        }

        [Test]
        public void DefinitionScriptableObjectsCanBeCreatedAsAssets()
        {
            AssetDatabase.CreateFolder("Assets", "RPGToolkit2DGeneratedTests");

            AssertCreated<CharacterDefinition>("Character.asset");
            AssertCreated<ItemDefinition>("Item.asset");
            AssertCreated<QuestDefinition>("Quest.asset");
            AssertCreated<DialogueDefinition>("Dialogue.asset");
            AssertCreated<AbilityDefinition>("Ability.asset");
            AssertCreated<StatDefinition>("Stat.asset");
            AssertCreated<ResourceDefinition>("Resource.asset");
            AssertCreated<ExperienceCurveDefinition>("ExperienceCurve.asset");
        }

        [Test]
        public void DatabaseValidationOutputIncludesDuplicateAssetIds()
        {
            var id = new RPGId("item.potion");
            var first = ScriptableObject.CreateInstance<ItemDefinition>();
            var second = ScriptableObject.CreateInstance<ItemDefinition>();
            first.name = "Potion A";
            second.name = "Potion B";
            first.SetId(id);
            second.SetId(id);

            var result = new RPGDatabase<ItemDefinition>(new[] { first, second }).Validate();

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Messages[0].Message, Does.Contain("Potion A").Or.Contain("Potion B"));
            Assert.That(result.Messages[0].RelatedId, Is.EqualTo(id));

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        private static void AssertCreated<T>(string fileName) where T : RPGObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var path = Path.Combine(TestFolder, fileName).Replace('\\', '/');
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            var loaded = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.Id.IsEmpty, Is.False);
        }
    }
}
