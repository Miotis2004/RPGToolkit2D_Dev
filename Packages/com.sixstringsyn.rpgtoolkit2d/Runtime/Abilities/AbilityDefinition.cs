using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Combat;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Abilities
{
    [System.Serializable]
    public sealed class AbilityResourceCost { public ResourceDefinition resource; public float amount; }

    [System.Serializable]
    public sealed class AbilityEffect
    {
        public DamageTypeDefinition damageType;
        public ResourceDefinition targetResource;
        public float amount;
        public StatDefinition attackStat;
        public StatDefinition defenseStat;
        public float criticalChance;
        public float criticalMultiplier = 1.5f;
    }

    [CreateAssetMenu(fileName = "NewAbilityDefinition", menuName = "RPG Toolkit/Ability Definition")]
    public sealed class AbilityDefinition : RPGObject
    {
        [SerializeField] private List<AbilityResourceCost> _costs = new List<AbilityResourceCost>();
        [SerializeField] private float _cooldownSeconds;
        [SerializeField] private float _range = 1f;
        [SerializeField] private TargetingMode _targetingMode = TargetingMode.SingleEnemy;
        [SerializeField] private List<AbilityEffect> _effects = new List<AbilityEffect>();

        public IReadOnlyList<AbilityResourceCost> Costs => _costs;
        public float CooldownSeconds => Mathf.Max(0f, _cooldownSeconds);
        public float Range => Mathf.Max(0f, _range);
        public TargetingMode TargetingMode => _targetingMode;
        public IReadOnlyList<AbilityEffect> Effects => _effects;

        protected override void OnValidate() { base.OnValidate(); _cooldownSeconds = Mathf.Max(0f, _cooldownSeconds); _range = Mathf.Max(0f, _range); }
    }
}
