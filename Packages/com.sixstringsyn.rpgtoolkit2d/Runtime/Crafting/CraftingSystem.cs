using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Core;

namespace SixStringSyn.RPGToolkit2D.Runtime.Crafting
{
    [RPGToolkitExperimental("Phase 13 release candidate: economy/crafting APIs need broader production feedback before stabilization.")]
    public sealed class CraftingResult { public bool Success; public string Message; public static CraftingResult Ok() => new CraftingResult { Success = true }; public static CraftingResult Fail(string message) => new CraftingResult { Success = false, Message = message }; }
    public interface ICraftingPresenter { void ShowRecipe(CraftingRecipeDefinition recipe, CraftingResult validation); void ShowCrafted(CraftingRecipeDefinition recipe); }
    [RPGToolkitExperimental("Phase 13 release candidate: economy/crafting APIs need broader production feedback before stabilization.")]
    public sealed class CraftingSystem
    {
        public CraftingResult Validate(CraftingRecipeDefinition recipe, InventoryContainer inventory, string stationId = null, int availableCurrency = 0)
        {
            if (recipe == null) return CraftingResult.Fail("Recipe is required."); if (inventory == null) return CraftingResult.Fail("Inventory is required.");
            if (!string.IsNullOrWhiteSpace(recipe.StationId) && !string.Equals(recipe.StationId, stationId, StringComparison.OrdinalIgnoreCase)) return CraftingResult.Fail("Wrong crafting station.");
            if (availableCurrency < recipe.CurrencyCost) return CraftingResult.Fail("Insufficient currency.");
            foreach (var ingredient in recipe.Ingredients) if (ingredient?.item == null || !inventory.Contains(ingredient.item, Math.Max(1, ingredient.quantity))) return CraftingResult.Fail("Missing ingredients.");
            if (recipe.Outputs.Count == 0) return CraftingResult.Fail("Recipe has no outputs."); return CraftingResult.Ok();
        }
        public CraftingResult Craft(CraftingRecipeDefinition recipe, InventoryContainer inventory, string stationId = null, int availableCurrency = 0)
        {
            var validation = Validate(recipe, inventory, stationId, availableCurrency); if (!validation.Success) return validation;
            foreach (var ingredient in recipe.Ingredients) inventory.Remove(ingredient.item, Math.Max(1, ingredient.quantity));
            var added = new List<Tuple<SixStringSyn.RPGToolkit2D.Runtime.Items.ItemDefinition, int>>();
            foreach (var output in recipe.Outputs)
            {
                var quantity = Math.Max(1, output.quantity); var placed = inventory.Add(output.item, quantity); added.Add(Tuple.Create(output.item, placed));
                if (placed < quantity) { foreach (var item in added) inventory.Remove(item.Item1, item.Item2); return CraftingResult.Fail("Not enough inventory space for outputs."); }
            }
            return CraftingResult.Ok();
        }
    }
}
