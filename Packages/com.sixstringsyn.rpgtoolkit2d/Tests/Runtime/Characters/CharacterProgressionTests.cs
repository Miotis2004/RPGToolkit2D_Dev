using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Characters
{
    public sealed class CharacterProgressionTests
    {
        [Test]
        public void ExperienceCurveReturnsExpectedLevels()
        {
            var curve = ScriptableObject.CreateInstance<ExperienceCurveDefinition>();
            Set(curve, "_xpRequiredPerLevel", new[] { 0, 100, 250 });

            Assert.That(curve.GetLevelForXp(99), Is.EqualTo(1));
            Assert.That(curve.GetLevelForXp(100), Is.EqualTo(2));
            Assert.That(curve.GetLevelForXp(999), Is.EqualTo(3));
            Object.DestroyImmediate(curve);
        }

        [Test]
        public void CharacterInstanceCopiesDefinitionDataAndRaisesLevelEvents()
        {
            var hpStat = ScriptableObject.CreateInstance<StatDefinition>();
            Set(hpStat, "_maximumValue", 999f);
            var health = ScriptableObject.CreateInstance<ResourceDefinition>();
            Set(health, "_maximumStat", hpStat);
            var curve = ScriptableObject.CreateInstance<ExperienceCurveDefinition>();
            Set(curve, "_xpRequiredPerLevel", new[] { 0, 50, 100 });
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            Set(definition, "_experienceCurve", curve);
            Set(definition, "_statTemplate", new List<CharacterStatEntry> { new CharacterStatEntry { stat = hpStat, value = 30f } });
            Set(definition, "_resources", new List<CharacterResourceEntry> { new CharacterResourceEntry { resource = health } });

            var instance = new CharacterInstance(definition);
            var levelEvents = 0;
            instance.LevelChanged += (_, before, after) => { levelEvents++; Assert.That((before, after), Is.EqualTo((1, 2))); };

            Assert.That(instance.Stats.GetValue(hpStat), Is.EqualTo(30f));
            Assert.That(instance.GetResource(health).Current, Is.EqualTo(30f));
            instance.AddExperience(60);
            Assert.That(instance.Level, Is.EqualTo(2));
            Assert.That(levelEvents, Is.EqualTo(1));

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(curve);
            Object.DestroyImmediate(health);
            Object.DestroyImmediate(hpStat);
        }

        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
