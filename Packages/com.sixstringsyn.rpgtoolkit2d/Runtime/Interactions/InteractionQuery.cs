using System.Collections.Generic;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    public static class InteractionQuery
    {
        public static IInteractable Best(IEnumerable<IInteractable> interactables, GameObject interactor = null)
        {
            if (interactables == null) return null;
            IInteractable best = null;
            foreach (var interactable in interactables)
            {
                if (interactable == null || !interactable.CanInteract(interactor)) continue;
                if (best == null || interactable.InteractionPriority > best.InteractionPriority) best = interactable;
            }
            return best;
        }
    }
}
