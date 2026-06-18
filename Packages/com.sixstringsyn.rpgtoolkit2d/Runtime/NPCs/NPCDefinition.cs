using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using SixStringSyn.RPGToolkit2D.Runtime.Vendors;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.NPCs
{
    [CreateAssetMenu(fileName = "NewNPCDefinition", menuName = "RPG Toolkit/NPC Definition")]
    public sealed class NPCDefinition : RPGObject
    {
        [SerializeField] private CharacterDefinition _character; [SerializeField] private DialogueDefinition _dialogue; [SerializeField] private VendorDefinition _vendor; [SerializeField] private bool _canRecruit;
        public CharacterDefinition Character => _character; public DialogueDefinition Dialogue => _dialogue; public VendorDefinition Vendor => _vendor; public bool CanRecruit => _canRecruit;
    }
    public sealed class NPCComponent : MonoBehaviour { [SerializeField] private NPCDefinition _definition; public NPCDefinition Definition => _definition; }
}
