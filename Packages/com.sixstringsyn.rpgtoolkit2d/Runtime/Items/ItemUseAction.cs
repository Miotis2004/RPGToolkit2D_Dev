using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Items
{
    public abstract class ItemUseAction : ScriptableObject
    {
        public abstract bool CanUse(ItemInstance item, object user, object context = null);
        public abstract bool Use(ItemInstance item, object user, object context = null);
    }
}
