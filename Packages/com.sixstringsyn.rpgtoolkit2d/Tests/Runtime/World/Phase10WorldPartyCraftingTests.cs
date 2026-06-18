using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Crafting;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Party;
using SixStringSyn.RPGToolkit2D.Runtime.Saving;
using SixStringSyn.RPGToolkit2D.Runtime.World;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.World
{
    public sealed class Phase10WorldPartyCraftingTests
    {
        [Test] public void WorldStateEvaluatesAndClearsValues()
        {
            var state = new WorldState(); state.SetFlag("door.locked", true); state.SetCounter("wolf.kills", 3);
            Assert.That(new WorldStateCondition { key = "door.locked" }.Evaluate(state), Is.True);
            Assert.That(new WorldStateCondition { key = "wolf.kills", comparison = WorldStateComparison.GreaterOrEqual, value = "2" }.Evaluate(state), Is.True);
            var context = new WorldStateDialogueContext(state); Assert.That(context.TryGetValue("wolf.kills", out var value), Is.True); Assert.That(value, Is.EqualTo("3"));
            Assert.That(state.Clear("door.locked"), Is.True); Assert.That(state.Has("door.locked"), Is.False);
        }
        [Test] public void PartyAddsRemovesActiveMembersAndRecruitsWithConditions()
        {
            var hero = Character("char.hero"); var companion = Character("char.mira"); var world = new WorldState(); world.SetFlag("quest.mira_helped");
            var party = new PartyRoster(); Assert.That(party.Add(hero), Is.True); Assert.That(party.SetActive(hero, true), Is.True);
            Assert.That(party.Recruit(companion, world, new[] { new WorldStateCondition { key = "quest.mira_helped" } }), Is.True);
            party.SetRelationship(companion, 25); Assert.That(party.GetRelationship(companion), Is.EqualTo(25)); Assert.That(party.Remove(hero), Is.True);
            Object.DestroyImmediate(hero); Object.DestroyImmediate(companion);
        }
        [Test] public void CraftingConsumesIngredientsAndProducesOutputs()
        {
            var herb = Item("item.herb", 20); var potion = Item("item.potion", 10); var recipe = ScriptableObject.CreateInstance<CraftingRecipeDefinition>();
            Set(recipe, "_stationId", "alchemy"); Set(recipe, "_ingredients", new System.Collections.Generic.List<CraftingIngredient> { new CraftingIngredient { item = herb, quantity = 2 } }); Set(recipe, "_outputs", new System.Collections.Generic.List<CraftingOutput> { new CraftingOutput { item = potion, quantity = 1 } });
            var inventory = new InventoryContainer(4); inventory.Add(herb, 3);
            Assert.That(new CraftingSystem().Craft(recipe, inventory, "alchemy").Success, Is.True); Assert.That(inventory.Count(herb), Is.EqualTo(1)); Assert.That(inventory.Count(potion), Is.EqualTo(1));
            Object.DestroyImmediate(herb); Object.DestroyImmediate(potion); Object.DestroyImmediate(recipe);
        }
        [Test] public void SaveContributorsRoundTripWorldAndParty()
        {
            var hero = Character("char.hero"); WorldState world = new WorldState(); world.SetCounter("gold", 5); PartyRoster party = new PartyRoster(); party.Add(hero); party.SetActive(hero, true);
            var service = new SaveGameService(); service.Register(new WorldStateSaveContributor(() => world, value => world = value)); service.Register(new PartySaveContributor(() => party, value => party = value, id => id == hero.Id.Value ? hero : null));
            var json = service.ToJson(service.Capture()); world = new WorldState(); party = new PartyRoster(); Assert.That(service.Restore(service.FromJson(json)).Success, Is.True);
            Assert.That(world.GetCounter("gold"), Is.EqualTo(5)); Assert.That(party.ActiveMembers.Count, Is.EqualTo(1)); Object.DestroyImmediate(hero);
        }
        private static CharacterDefinition Character(string id) { var c = ScriptableObject.CreateInstance<CharacterDefinition>(); c.SetId(new RPGId(id)); return c; }
        private static ItemDefinition Item(string id, int stack) { var i = ScriptableObject.CreateInstance<ItemDefinition>(); i.SetId(new RPGId(id)); Set(i, "_maximumStackSize", stack); return i; }
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
