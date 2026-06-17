using System;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    public sealed class NPCInteraction : InteractableBehaviour
    {
        [SerializeField] private CharacterDefinition _character;
        [SerializeField] private DialogueDefinition _dialogue;
        public CharacterDefinition Character => _character;
        public DialogueDefinition Dialogue => _dialogue;
        public event Action<GameObject, NPCInteraction> Interacted;
        public override void Interact(GameObject interactor) => Interacted?.Invoke(interactor, this);
    }
}
