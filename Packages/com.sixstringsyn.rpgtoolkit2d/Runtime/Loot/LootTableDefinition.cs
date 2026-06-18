using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Loot
{
    [Serializable] [RPGToolkitExperimental("Phase 13 release candidate: economy/crafting APIs need broader production feedback before stabilization.")]
    public sealed class LootEntry { public ItemDefinition item; public int minQuantity = 1; public int maxQuantity = 1; public int weight = 1; }
    [CreateAssetMenu(fileName = "NewLootTable", menuName = "RPG Toolkit/Loot Table")]
    [RPGToolkitExperimental("Phase 13 release candidate: economy/crafting APIs need broader production feedback before stabilization.")]
    public sealed class LootTableDefinition : RPGObject { [SerializeField] private List<LootEntry> _entries = new List<LootEntry>(); public IReadOnlyList<LootEntry> Entries => _entries; }
    public static class LootRoller
    {
        public static ItemInstance Roll(LootTableDefinition table, System.Random random = null)
        {
            if (table == null || table.Entries.Count == 0) return null; if (random == null) random = new System.Random(); var total = 0; foreach (var e in table.Entries) total += Math.Max(0, e.weight); if (total <= 0) return null;
            var roll = random.Next(total); foreach (var e in table.Entries) { roll -= Math.Max(0, e.weight); if (roll < 0) return e.item == null ? null : new ItemInstance(e.item, random.Next(Math.Max(1, e.minQuantity), Math.Max(Math.Max(1, e.minQuantity), e.maxQuantity) + 1)); }
            return null;
        }
    }
}
