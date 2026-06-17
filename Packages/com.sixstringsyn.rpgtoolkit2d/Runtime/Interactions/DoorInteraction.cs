using SixStringSyn.RPGToolkit2D.Runtime.World;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    public sealed class DoorInteraction : InteractableBehaviour
    {
        [SerializeField] private string _targetSceneName;
        [SerializeField] private string _targetSpawnPointId;
        [SerializeField] private bool _saveBeforeTransition;
        public SceneTransitionRequest CreateRequest() => new SceneTransitionRequest(_targetSceneName, _targetSpawnPointId, _saveBeforeTransition);
        public override bool CanInteract(GameObject interactor) => base.CanInteract(interactor) && CreateRequest().IsValid;
        public override void Interact(GameObject interactor) => SceneTransitionService.Default.RequestTransition(CreateRequest());
        public override bool Validate(out string message) { if (!base.Validate(out message)) return false; if (string.IsNullOrWhiteSpace(_targetSceneName)) { message = $"{name} is missing a target scene."; return false; } return true; }
    }
}
