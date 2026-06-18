using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Crafting
{
    [System.Serializable] public sealed class CraftingStationDefinition { public string stationId; public string displayName; public Sprite icon; }
    [CreateAssetMenu(fileName = "CraftingRecipeDatabase", menuName = "RPG Toolkit/Crafting/Recipe Database")]
    public sealed class CraftingDatabase : ScriptableObject
    {
        [SerializeField] private List<CraftingStationDefinition> _stations = new List<CraftingStationDefinition>();
        [SerializeField] private List<CraftingRecipeDefinition> _recipes = new List<CraftingRecipeDefinition>();
        public IReadOnlyList<CraftingStationDefinition> Stations => _stations; public IReadOnlyList<CraftingRecipeDefinition> Recipes => _recipes;
        public IEnumerable<CraftingRecipeDefinition> RecipesForStation(string stationId) { foreach (var r in _recipes) if (r != null && (string.IsNullOrWhiteSpace(r.StationId) || string.Equals(r.StationId, stationId, System.StringComparison.OrdinalIgnoreCase))) yield return r; }
        public RPGValidationResult ValidateDatabase() { var result = new RPGValidationResult(); var ids = new HashSet<string>(); foreach (var s in _stations) if (s == null || string.IsNullOrWhiteSpace(s.stationId)) result.AddError("crafting.station.id", "Crafting station needs an id."); else if (!ids.Add(s.stationId)) result.AddError("crafting.station.duplicate", $"Duplicate crafting station '{s.stationId}'."); foreach (var r in _recipes) if (r == null) result.AddError("crafting.recipe.missing", "Recipe database contains an empty recipe slot."); else foreach (var message in r.ValidateRecipe().Messages) result.Add(message.Severity, message.Code, message.Message, message.RelatedId); return result; }
    }
}
