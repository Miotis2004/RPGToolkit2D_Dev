using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Crafting
{
    [System.Serializable] [RPGToolkitExperimental("Phase 13 release candidate: economy/crafting APIs need broader production feedback before stabilization.")]
    public sealed class CraftingIngredient { public ItemDefinition item; public int quantity = 1; }
    [System.Serializable] public sealed class CraftingOutput { public ItemDefinition item; public int quantity = 1; }
    [CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "RPG Toolkit/Crafting Recipe")]
    [RPGToolkitExperimental("Phase 13 release candidate: economy/crafting APIs need broader production feedback before stabilization.")]
    public sealed class CraftingRecipeDefinition : RPGObject
    {
        [SerializeField] private string _stationId;
        [SerializeField] private int _currencyCost;
        [SerializeField] private List<CraftingIngredient> _ingredients = new List<CraftingIngredient>();
        [SerializeField] private List<CraftingOutput> _outputs = new List<CraftingOutput>();
        public string StationId => _stationId ?? string.Empty; public int CurrencyCost => Mathf.Max(0, _currencyCost); public IReadOnlyList<CraftingIngredient> Ingredients => _ingredients; public IReadOnlyList<CraftingOutput> Outputs => _outputs;
        public RPGValidationResult ValidateRecipe() { var r = new RPGValidationResult(); if (_ingredients.Count == 0) r.AddWarning("crafting.ingredients.empty", "Recipe has no ingredients.", Id); if (_outputs.Count == 0) r.AddError("crafting.outputs.empty", "Recipe needs at least one output.", Id); foreach (var i in _ingredients) { if (i == null || i.item == null) r.AddError("crafting.ingredient.item", "Recipe contains a missing ingredient item.", Id); else if (i.quantity <= 0) r.AddError("crafting.ingredient.quantity", "Ingredient quantity must be greater than zero.", Id); } foreach (var o in _outputs) { if (o == null || o.item == null) r.AddError("crafting.output.item", "Recipe contains a missing output item.", Id); else if (o.quantity <= 0) r.AddError("crafting.output.quantity", "Output quantity must be greater than zero.", Id); } return r; }
        protected override void OnValidate() { base.OnValidate(); _currencyCost = Mathf.Max(0, _currencyCost); foreach (var i in _ingredients) if (i != null) i.quantity = Mathf.Max(1, i.quantity); foreach (var o in _outputs) if (o != null) o.quantity = Mathf.Max(1, o.quantity); }
    }
}
