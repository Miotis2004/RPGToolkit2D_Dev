using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Core
{
    public sealed class RPGFoundationTests
    {
        [Test]
        public void ContentReferencesIncludeKindAndStableId()
        {
            var reference = new RPGContentReference(RPGContentKind.Quest, new RPGId("quest.intro"));

            Assert.That(reference.IsEmpty, Is.False);
            Assert.That(reference.ToString(), Is.EqualTo("Quest:quest.intro"));
        }

        [Test]
        public void SchemaVersionsCompareAndCheckMajorCompatibility()
        {
            var current = new RPGSchemaVersion(1, 2, 0);

            Assert.That(current.IsCompatibleWith(new RPGSchemaVersion(1, 0, 0)), Is.True);
            Assert.That(current.IsCompatibleWith(new RPGSchemaVersion(2, 0, 0)), Is.False);
            Assert.That(current.CompareTo(new RPGSchemaVersion(1, 1, 9)), Is.GreaterThan(0));
        }

        [Test]
        public void ContentManifestReportsDuplicateStableIds()
        {
            var id = new RPGId("character.hero");
            var manifest = ScriptableObject.CreateInstance<RPGContentManifest>();
            var first = ScriptableObject.CreateInstance<CharacterDefinition>();
            var second = ScriptableObject.CreateInstance<CharacterDefinition>();
            first.name = "Hero A";
            second.name = "Hero B";
            first.SetId(id);
            second.SetId(id);

            manifest.SetContent(new RPGObject[] { first, second });

            var result = manifest.Validate();

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Messages, Has.Exactly(1).Matches<RPGValidationMessage>(message => message.Code == "RPG_MANIFEST_DUPLICATE_ID"));

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(manifest);
        }
    }
}
