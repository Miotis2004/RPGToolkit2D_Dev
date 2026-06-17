using System;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Core
{
    public sealed class RPGCoreModelTests
    {
        [Serializable]
        private sealed class IdContainer
        {
            public RPGId id;
        }

        [Test]
        public void NewIdCreatesStableNonEmptyIdentifiers()
        {
            var first = RPGId.NewId();
            var second = RPGId.NewId();

            Assert.That(first.IsEmpty, Is.False);
            Assert.That(second.IsEmpty, Is.False);
            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void IdEqualityIgnoresCaseAndWhitespace()
        {
            Assert.That(new RPGId(" Hero.Id "), Is.EqualTo(new RPGId("hero.id")));
            Assert.That(new RPGId("Hero.Id").GetHashCode(), Is.EqualTo(new RPGId("hero.id").GetHashCode()));
        }

        [Test]
        public void IdSerializesThroughUnityJsonUtility()
        {
            var source = new IdContainer { id = new RPGId("quest.main") };
            var json = JsonUtility.ToJson(source);
            var restored = JsonUtility.FromJson<IdContainer>(json);

            Assert.That(restored.id, Is.EqualTo(source.id));
        }

        [Test]
        public void TagQueriesMatchAllAndAnyRequirements()
        {
            var tags = new[] { new RPGTag("Element.Fire"), new RPGTag("Boss") };

            Assert.That(RPGTagQuery.HasTag(tags, new RPGTag("element.fire")), Is.True);
            Assert.That(RPGTagQuery.HasAll(tags, new[] { new RPGTag("boss"), new RPGTag("element.fire") }), Is.True);
            Assert.That(RPGTagQuery.HasAny(tags, new[] { new RPGTag("npc"), new RPGTag("boss") }), Is.True);
            Assert.That(RPGTagQuery.HasAny(tags, new[] { new RPGTag("npc") }), Is.False);
        }

        [Test]
        public void DatabaseValidationReportsDuplicateIds()
        {
            var id = new RPGId("character.hero");
            var first = ScriptableObject.CreateInstance<CharacterDefinition>();
            var second = ScriptableObject.CreateInstance<CharacterDefinition>();
            first.name = "Hero A";
            second.name = "Hero B";
            first.SetId(id);
            second.SetId(id);

            var database = new RPGDatabase<CharacterDefinition>(new[] { first, second });
            var result = database.Validate();

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Messages, Has.Exactly(1).Matches<RPGValidationMessage>(message => message.Code == "RPGDB_DUPLICATE_ID"));

            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
        }
    }
}
