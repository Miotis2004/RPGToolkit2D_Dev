using System.Collections.Generic;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public interface IDialoguePresenter
    {
        void ShowLine(DialogueNode node, IReadOnlyList<DialogueChoice> availableChoices);
        void HideDialogue();
    }
}
