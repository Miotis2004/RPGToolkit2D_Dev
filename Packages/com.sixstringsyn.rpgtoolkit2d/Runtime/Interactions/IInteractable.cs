using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    public interface IInteractable
    {
        string InteractionLabel { get; }
        int InteractionPriority { get; }
        bool CanInteract(GameObject interactor);
        void Interact(GameObject interactor);
    }
}
