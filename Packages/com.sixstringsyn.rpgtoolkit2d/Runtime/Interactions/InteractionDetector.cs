using System;
using System.Collections.Generic;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Interactions
{
    public sealed class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private float _radius = 1.25f;
        [SerializeField] private LayerMask _layers = ~0;
        [SerializeField] private IInteractionPrompt _prompt;
        private readonly List<IInteractable> _candidates = new List<IInteractable>();
        private readonly Collider2D[] _hits = new Collider2D[16];
        public IInteractable Current { get; private set; }
        public event Action<IInteractable> CurrentChanged;

        public void SetPrompt(IInteractionPrompt prompt) => _prompt = prompt;
        public void Register(IInteractable interactable) { if (interactable != null && !_candidates.Contains(interactable)) _candidates.Add(interactable); Refresh(); }
        public void Unregister(IInteractable interactable) { if (_candidates.Remove(interactable)) Refresh(); }
        public void ScanOverlap() { _candidates.Clear(); var count = Physics2D.OverlapCircleNonAlloc(transform.position, _radius, _hits, _layers); for (var i=0;i<count;i++) AddFrom(_hits[i]); Refresh(); }
        public void ScanRay(Vector2 direction, float distance) { _candidates.Clear(); var hit = Physics2D.Raycast(transform.position, direction, distance, _layers); if (hit.collider != null) AddFrom(hit.collider); Refresh(); }
        public void Interact() { if (Current != null && Current.CanInteract(gameObject)) Current.Interact(gameObject); Refresh(); }
        private void AddFrom(Component component) { if (component != null) { var interactable = component.GetComponentInParent<IInteractable>(); if (interactable != null && !_candidates.Contains(interactable)) _candidates.Add(interactable); } }
        public void Refresh()
        {
            _candidates.RemoveAll(i => i == null || !i.CanInteract(gameObject));
            var next = InteractionQuery.Best(_candidates, gameObject);
            if (ReferenceEquals(next, Current)) return;
            if (Current != null) _prompt?.Hide(Current);
            Current = next;
            if (Current != null) _prompt?.Show(Current.InteractionLabel, Current);
            CurrentChanged?.Invoke(Current);
        }
        private void OnTriggerEnter2D(Collider2D other) => AddFrom(other);
        private void OnTriggerExit2D(Collider2D other) { var interactable = other.GetComponentInParent<IInteractable>(); if (interactable != null) Unregister(interactable); }
    }
}
