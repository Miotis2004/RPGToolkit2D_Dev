using SixStringSyn.RPGToolkit2D.Runtime.Interactions;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public sealed class NPCDialogueAdapter : MonoBehaviour
    {
        [SerializeField] private NPCInteraction _npcInteraction;
        private DialogueRunner _runner;
        public DialogueRunner Runner => _runner;

        private void Awake()
        {
            if (_npcInteraction == null) _npcInteraction = GetComponent<NPCInteraction>();
            _runner = new DialogueRunner(GetComponent<IDialoguePresenter>());
        }

        private void OnEnable()
        {
            if (_npcInteraction != null) _npcInteraction.Interacted += OnInteracted;
        }

        private void OnDisable()
        {
            if (_npcInteraction != null) _npcInteraction.Interacted -= OnInteracted;
        }

        private void OnInteracted(GameObject interactor, NPCInteraction npc)
        {
            if (npc.Dialogue != null) _runner.Start(npc.Dialogue);
        }
    }
}
