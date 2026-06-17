using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Stats
{
    public sealed class StatAndResourceTests
    {
        [Test]
        public void StatBlockAppliesModifiersInDeterministicOrder()
        {
            var strength = Stat(0f, 0f, 999f);
            var block = new StatBlock(new[] { new StatValue(strength, 10f) });

            block.AddModifier(strength, new StatModifier(StatModifierType.Multiplicative, 2f));
            block.AddModifier(strength, new StatModifier(StatModifierType.Additive, 5f));
            block.AddModifier(strength, new StatModifier(StatModifierType.Override, 40f));

            Assert.That(block.GetValue(strength), Is.EqualTo(40f));
            Object.DestroyImmediate(strength);
        }

        [Test]
        public void ConditionalAndTemporaryModifiersCanBeToggledAndRemoved()
        {
            var defense = Stat(0f, 0f, 999f);
            var enraged = new RPGTag("state.enraged");
            var block = new StatBlock(new[] { new StatValue(defense, 10f) });
            block.AddModifier(defense, new StatModifier(StatModifierType.Additive, 15f, requiredTag: enraged));
            block.AddModifier(defense, new StatModifier(StatModifierType.Additive, 5f, isTemporary: true));

            Assert.That(block.GetValue(defense), Is.EqualTo(15f));
            block.SetContextTags(new[] { enraged });
            Assert.That(block.GetValue(defense), Is.EqualTo(30f));
            Assert.That(block.RemoveTemporaryModifiers(), Is.EqualTo(1));
            Assert.That(block.GetValue(defense), Is.EqualTo(25f));
            Object.DestroyImmediate(defense);
        }

        [Test]
        public void ResourcePoolClampsConsumeRestoreAndRegenerate()
        {
            var resource = ScriptableObject.CreateInstance<ResourceDefinition>();
            Set(resource, "_regenerationPerSecond", 3f);
            var pool = new ResourcePool(resource, 10f);

            Assert.That(pool.Consume(12f), Is.EqualTo(10f));
            Assert.That(pool.Current, Is.EqualTo(0f));
            Assert.That(pool.Restore(20f), Is.EqualTo(10f));
            pool.Consume(5f);
            Assert.That(pool.Regenerate(1f), Is.EqualTo(3f));
            Assert.That(pool.Current, Is.EqualTo(8f));
            Object.DestroyImmediate(resource);
        }

        private static StatDefinition Stat(float defaultValue, float min, float max)
        {
            var stat = ScriptableObject.CreateInstance<StatDefinition>();
            Set(stat, "_defaultValue", defaultValue);
            Set(stat, "_minimumValue", min);
            Set(stat, "_maximumValue", max);
            return stat;
        }

        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
