using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    public sealed class TriggerInteraction : InteractableBehaviour
    {
        public event Action<GameObject, TriggerInteraction> Triggered;
        public override void Interact(GameObject interactor) => Triggered?.Invoke(interactor, this);
    }
}
