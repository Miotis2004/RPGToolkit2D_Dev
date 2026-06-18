using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Combat;

namespace SixStringSyn.RPGToolkit2D.Runtime.Abilities
{
    public sealed class AbilityExecutor
    {
        private readonly Dictionary<AbilityDefinition, float> _cooldowns = new Dictionary<AbilityDefinition, float>();
        private readonly ICombatTargetValidator _targetValidator;
        public AbilityExecutor(ICombatTargetValidator targetValidator = null) { _targetValidator = targetValidator; }
        public bool CanUse(AbilityDefinition ability, CharacterInstance source, CharacterInstance target, float time)
        {
            if (ability == null || source == null) return false;
            if (_cooldowns.TryGetValue(ability, out var readyAt) && time < readyAt) return false;
            foreach (var cost in ability.Costs)
            {
                var pool = source.GetResource(cost.resource);
                if (pool == null || pool.Current < cost.amount) return false;
            }
            return _targetValidator == null || _targetValidator.IsValidTarget(source, target, ability.TargetingMode, ability.Range);
        }
        public IReadOnlyList<DamageResult> Use(AbilityDefinition ability, CharacterInstance source, CharacterInstance target, float time, Random random = null)
        {
            if (!CanUse(ability, source, target, time)) return Array.Empty<DamageResult>();
            foreach (var cost in ability.Costs) source.GetResource(cost.resource)?.Consume(cost.amount);
            _cooldowns[ability] = time + ability.CooldownSeconds;
            var results = new List<DamageResult>();
            foreach (var effect in ability.Effects)
                results.Add(DamageCalculator.Apply(new DamageRequest { Source = source, Target = target, DamageType = effect.damageType, TargetResource = effect.targetResource, Amount = effect.amount, AttackStat = effect.attackStat, DefenseStat = effect.defenseStat, CriticalChance = effect.criticalChance, CriticalMultiplier = effect.criticalMultiplier }, random));
            return results;
        }
    }

    public sealed class MeleeAttackAdapter { private readonly AbilityExecutor _executor; public MeleeAttackAdapter(AbilityExecutor executor = null) { _executor = executor ?? new AbilityExecutor(); } public IReadOnlyList<DamageResult> Attack(AbilityDefinition ability, CharacterInstance source, CharacterInstance target, float time) => _executor.Use(ability, source, target, time); }
    public sealed class ProjectileAttackAdapter { private readonly IHitDetector _hitDetector; private readonly AbilityExecutor _executor; public ProjectileAttackAdapter(IHitDetector hitDetector, AbilityExecutor executor = null) { _hitDetector = hitDetector; _executor = executor ?? new AbilityExecutor(); } public bool TryResolve(AbilityDefinition ability, CharacterInstance source, object origin, object projectile, float time, out IReadOnlyList<DamageResult> results) { results = Array.Empty<DamageResult>(); if (_hitDetector == null || !_hitDetector.TryGetTarget(origin, projectile, out var target)) return false; results = _executor.Use(ability, source, target, time); return results.Count > 0; } }
}
