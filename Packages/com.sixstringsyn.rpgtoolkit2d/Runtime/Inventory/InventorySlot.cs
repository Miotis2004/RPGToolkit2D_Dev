using SixStringSyn.RPGToolkit2D.Runtime.Items;

namespace SixStringSyn.RPGToolkit2D.Runtime.Inventory
{
    public sealed class InventorySlot
    {
        public InventorySlot(int index) { Index = index; }
        public int Index { get; }
        public ItemInstance Item { get; internal set; }
        public bool IsEmpty => Item == null || Item.IsEmpty;
        internal void Clear() => Item = null;
    }
}
