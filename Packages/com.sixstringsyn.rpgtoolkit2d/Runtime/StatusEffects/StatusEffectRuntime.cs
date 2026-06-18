using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Combat;

namespace SixStringSyn.RPGToolkit2D.Runtime.StatusEffects
{
    public sealed class StatusEffectInstance { public StatusEffectInstance(StatusEffectDefinition definition) { Definition = definition; RemainingSeconds = definition.DurationSeconds; Stacks = 1; } public StatusEffectDefinition Definition { get; } public float RemainingSeconds { get; set; } public int Stacks { get; set; } public float TickTimer { get; set; } }
    public sealed class StatusEffectController
    {
        private readonly List<StatusEffectInstance> _effects = new List<StatusEffectInstance>();
        public IReadOnlyList<StatusEffectInstance> Effects => _effects;
        public StatusEffectInstance Apply(StatusEffectDefinition definition)
        {
            var existing = _effects.Find(e => e.Definition == definition);
            if (existing == null) { existing = new StatusEffectInstance(definition); _effects.Add(existing); return existing; }
            if (definition.StackingMode == StatusStackingMode.AddDuration) existing.RemainingSeconds += definition.DurationSeconds;
            else if (definition.StackingMode == StatusStackingMode.StackIntensity) existing.Stacks = System.Math.Min(definition.MaximumStacks, existing.Stacks + 1);
            else if (definition.StackingMode == StatusStackingMode.RefreshDuration) existing.RemainingSeconds = definition.DurationSeconds;
            return existing;
        }
        public void Tick(float deltaSeconds, CharacterInstance owner, CharacterInstance source = null)
        {
            for (var i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i]; effect.RemainingSeconds -= deltaSeconds; effect.TickTimer += deltaSeconds;
                if (effect.Definition.TickIntervalSeconds > 0f && effect.TickTimer >= effect.Definition.TickIntervalSeconds) { effect.TickTimer = 0f; var p = effect.Definition.PeriodicEffect; if (p != null) DamageCalculator.Apply(new DamageRequest { Source = source, Target = owner, DamageType = p.damageType, TargetResource = p.targetResource, Amount = p.amount * effect.Stacks, AttackStat = p.attackStat, DefenseStat = p.defenseStat }); }
                if (effect.Definition.RemovalRule == StatusRemovalRule.Expires && effect.RemainingSeconds <= 0f) _effects.RemoveAt(i);
            }
        }
        public bool Remove(StatusEffectDefinition definition) => _effects.RemoveAll(e => e.Definition == definition) > 0;
    }
}
