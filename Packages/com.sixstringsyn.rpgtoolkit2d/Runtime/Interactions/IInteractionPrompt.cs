namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    public interface IInteractionPrompt
    {
        void Show(string label, IInteractable interactable);
        void Hide(IInteractable interactable);
    }
}
