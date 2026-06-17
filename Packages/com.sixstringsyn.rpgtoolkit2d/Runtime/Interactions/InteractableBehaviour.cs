using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    public abstract class InteractableBehaviour : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _interactionLabel = "Interact";
        [SerializeField] private int _interactionPriority;
        [SerializeField] private bool _isInteractable = true;

        public string InteractionLabel => _interactionLabel;
        public int InteractionPriority => _interactionPriority;
        public bool IsInteractable { get => _isInteractable; set => _isInteractable = value; }

        public virtual bool CanInteract(GameObject interactor) => isActiveAndEnabled && _isInteractable;
        public abstract void Interact(GameObject interactor);

        public virtual bool Validate(out string message)
        {
            if (string.IsNullOrWhiteSpace(_interactionLabel))
            {
                message = $"{name} is missing an interaction label.";
                return false;
            }
            message = string.Empty;
            return true;
        }
    }
}
