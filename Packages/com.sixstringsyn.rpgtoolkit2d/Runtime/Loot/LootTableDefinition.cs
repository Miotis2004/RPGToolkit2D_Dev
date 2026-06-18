using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Loot
{
    [Serializable] [RPGToolkitExperimental("Phase 5 advanced gameplay editor API; schema may evolve with visual tooling feedback.")]
    public sealed class LootEntry { public ItemDefinition item; public LootTableDefinition nestedTable; public int minQuantity = 1; public int maxQuantity = 1; public int weight = 1; }
    [Serializable] public sealed class LootSimulationResult { public int rolls; public List<ItemInstance> drops = new List<ItemInstance>(); }
    [CreateAssetMenu(fileName = "NewLootTable", menuName = "RPG Toolkit/Loot Table")]
    [RPGToolkitExperimental("Phase 5 advanced gameplay editor API; schema may evolve with visual tooling feedback.")]
    public sealed class LootTableDefinition : RPGObject
    {
        [SerializeField] private List<LootEntry> _entries = new List<LootEntry>();
        public IReadOnlyList<LootEntry> Entries => _entries;
        public RPGValidationResult ValidateTable() { var r = new RPGValidationResult(); if (_entries.Count == 0) r.AddWarning("loot.empty", "Loot table has no entries.", Id); foreach (var e in _entries) { if (e == null) { r.AddError("loot.entry.missing", "Loot table contains an empty entry.", Id); continue; } if (e.item == null && e.nestedTable == null) r.AddError("loot.entry.reward", "Loot entry needs an item or nested table.", Id); if (e.weight <= 0) r.AddError("loot.entry.weight", "Loot entry weight must be greater than zero.", Id); if (e.maxQuantity < e.minQuantity) r.AddError("loot.entry.quantity", "Loot entry max quantity must be at least min quantity.", Id); } return r; }
        protected override void OnValidate() { base.OnValidate(); foreach (var e in _entries) if (e != null) { e.minQuantity = Mathf.Max(1, e.minQuantity); e.maxQuantity = Mathf.Max(e.minQuantity, e.maxQuantity); e.weight = Mathf.Max(1, e.weight); } }
    }
    public static class LootRoller
    {
        public static ItemInstance Roll(LootTableDefinition table, System.Random random = null, int maxDepth = 8)
        {
            if (table == null || table.Entries.Count == 0 || maxDepth < 0) return null; if (random == null) random = new System.Random(); var total = 0; foreach (var e in table.Entries) total += Math.Max(0, e.weight); if (total <= 0) return null;
            var roll = random.Next(total); foreach (var e in table.Entries) { roll -= Math.Max(0, e.weight); if (roll < 0) { if (e.nestedTable != null) return Roll(e.nestedTable, random, maxDepth - 1); return e.item == null ? null : new ItemInstance(e.item, random.Next(Math.Max(1, e.minQuantity), Math.Max(Math.Max(1, e.minQuantity), e.maxQuantity) + 1)); } }
            return null;
        }
        public static LootSimulationResult Simulate(LootTableDefinition table, int rolls, int seed = 0) { var result = new LootSimulationResult { rolls = Math.Max(0, rolls) }; var random = seed == 0 ? new System.Random() : new System.Random(seed); for (var i = 0; i < result.rolls; i++) { var drop = Roll(table, random); if (drop != null) result.drops.Add(drop); } return result; }
    }
}
