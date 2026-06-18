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
        protected override void OnValidate() { base.OnValidate(); _currencyCost = Mathf.Max(0, _currencyCost); foreach (var i in _ingredients) if (i != null) i.quantity = Mathf.Max(1, i.quantity); foreach (var o in _outputs) if (o != null) o.quantity = Mathf.Max(1, o.quantity); }
    }
}
