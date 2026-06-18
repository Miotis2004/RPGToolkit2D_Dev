using SixStringSyn.RPGToolkit2D.Runtime.Abilities;
using SixStringSyn.RPGToolkit2D.Runtime.Combat;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.StatusEffects
{
    public enum StatusStackingMode { RefreshDuration, AddDuration, StackIntensity, Ignore }
    public enum StatusRemovalRule { Expires, Manual, OnCombatEnd }

    [CreateAssetMenu(fileName = "NewStatusEffect", menuName = "RPG Toolkit/Combat/Status Effect")]
    public sealed class StatusEffectDefinition : RPGObject
    {
        [SerializeField] private float _durationSeconds = 5f;
        [SerializeField] private int _maximumStacks = 1;
        [SerializeField] private float _tickIntervalSeconds = 1f;
        [SerializeField] private StatusStackingMode _stackingMode;
        [SerializeField] private StatusRemovalRule _removalRule;
        [SerializeField] private StatDefinition _modifiedStat;
        [SerializeField] private float _statModifierAmount;
        [SerializeField] private AbilityEffect _periodicEffect;
        public float DurationSeconds => Mathf.Max(0f, _durationSeconds);
        public int MaximumStacks => Mathf.Max(1, _maximumStacks);
        public float TickIntervalSeconds => Mathf.Max(0f, _tickIntervalSeconds);
        public StatusStackingMode StackingMode => _stackingMode;
        public StatusRemovalRule RemovalRule => _removalRule;
        public StatDefinition ModifiedStat => _modifiedStat;
        public float StatModifierAmount => _statModifierAmount;
        public AbilityEffect PeriodicEffect => _periodicEffect;
    }
}
