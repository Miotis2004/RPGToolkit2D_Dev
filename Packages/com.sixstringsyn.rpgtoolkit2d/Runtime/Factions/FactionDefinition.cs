using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Factions
{
    [System.Serializable]
    public sealed class ReputationThreshold { public string id; public int minimumValue; public string displayName; }

    [CreateAssetMenu(fileName = "NewFaction", menuName = "RPG Toolkit/Factions/Faction")]
    public sealed class FactionDefinition : RPGObject
    {
        [SerializeField] private int _minimumReputation = -100;
        [SerializeField] private int _maximumReputation = 100;
        [SerializeField] private List<ReputationThreshold> _thresholds = new List<ReputationThreshold>();
        public int MinimumReputation => _minimumReputation; public int MaximumReputation => _maximumReputation; public IReadOnlyList<ReputationThreshold> Thresholds => _thresholds;
        public int Clamp(int value) => Mathf.Clamp(value, _minimumReputation, _maximumReputation);
        public ReputationThreshold ResolveThreshold(int value) { ReputationThreshold best = null; foreach (var t in _thresholds) if (t != null && value >= t.minimumValue && (best == null || t.minimumValue > best.minimumValue)) best = t; return best; }
        protected override void OnValidate() { base.OnValidate(); if (_maximumReputation < _minimumReputation) _maximumReputation = _minimumReputation; }
    }
}
