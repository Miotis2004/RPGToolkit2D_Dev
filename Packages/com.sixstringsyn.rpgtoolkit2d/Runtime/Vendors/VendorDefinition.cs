using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Vendors
{
    [Serializable] public sealed class VendorStockEntry { public ItemDefinition item; public int quantity = 1; public int price = 1; }
    [CreateAssetMenu(fileName = "NewVendor", menuName = "RPG Toolkit/Vendor")]
    public sealed class VendorDefinition : RPGObject { [SerializeField] private List<VendorStockEntry> _stock = new List<VendorStockEntry>(); [SerializeField] private float _sellMultiplier = .5f; public IReadOnlyList<VendorStockEntry> Stock => _stock; public float SellMultiplier => Mathf.Clamp01(_sellMultiplier); }
    public sealed class VendorShop
    {
        private readonly VendorDefinition _definition; private readonly Dictionary<ItemDefinition, int> _stock = new Dictionary<ItemDefinition, int>(); public Func<ItemDefinition, int, int> PricingRule; public Action Restocked;
        public VendorShop(VendorDefinition definition) { _definition = definition; Restock(); }
        public void Restock() { _stock.Clear(); if (_definition != null) foreach (var e in _definition.Stock) if (e.item != null) _stock[e.item] = Math.Max(0, e.quantity); Restocked?.Invoke(); }
        public int GetBuyPrice(ItemDefinition item) { var e = Find(item); return PricingRule?.Invoke(item, e?.price ?? 0) ?? (e?.price ?? 0); }
        public int GetSellPrice(ItemDefinition item) => (int)Math.Round(GetBuyPrice(item) * (_definition?.SellMultiplier ?? .5f));
        public bool CanBuy(ItemDefinition item, int quantity, int buyerCurrency) => item != null && quantity > 0 && _stock.TryGetValue(item, out var available) && available >= quantity && buyerCurrency >= GetBuyPrice(item) * quantity;
        public bool Buy(ItemDefinition item, int quantity, InventoryContainer buyerInventory, ref int buyerCurrency) { if (!CanBuy(item, quantity, buyerCurrency) || buyerInventory == null) return false; var added = buyerInventory.Add(item, quantity); if (added != quantity) { if (added > 0) buyerInventory.Remove(item, added); return false; } _stock[item] -= quantity; buyerCurrency -= GetBuyPrice(item) * quantity; return true; }
        public bool Sell(ItemDefinition item, int quantity, InventoryContainer sellerInventory, ref int sellerCurrency) { if (item == null || quantity <= 0 || sellerInventory == null || !sellerInventory.Contains(item, quantity)) return false; sellerInventory.Remove(item, quantity); sellerCurrency += GetSellPrice(item) * quantity; _stock[item] = (_stock.TryGetValue(item, out var current) ? current : 0) + quantity; return true; }
        private VendorStockEntry Find(ItemDefinition item) { if (_definition == null) return null; foreach (var e in _definition.Stock) if (e.item == item) return e; return null; }
    }
}
