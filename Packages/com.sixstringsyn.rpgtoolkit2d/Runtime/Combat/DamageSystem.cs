using System;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Combat
{
    public enum TargetingMode { Self, SingleAlly, SingleEnemy, Area, Any }

    [CreateAssetMenu(fileName = "NewDamageType", menuName = "RPG Toolkit/Combat/Damage Type")]
    public sealed class DamageTypeDefinition : RPGObject { }

    [Serializable]
    public sealed class DamageRequest
    {
        public CharacterInstance Source;
        public CharacterInstance Target;
        public DamageTypeDefinition DamageType;
        public ResourceDefinition TargetResource;
        public float Amount;
        public StatDefinition AttackStat;
        public StatDefinition DefenseStat;
        public float CriticalChance;
        public float CriticalMultiplier = 1.5f;
        public Func<DamageContext, float> MitigationHook;
    }

    public readonly struct DamageResult
    {
        public DamageResult(float rawAmount, float mitigatedAmount, bool critical, float appliedAmount)
        { RawAmount = rawAmount; MitigatedAmount = mitigatedAmount; Critical = critical; AppliedAmount = appliedAmount; }
        public float RawAmount { get; }
        public float MitigatedAmount { get; }
        public bool Critical { get; }
        public float AppliedAmount { get; }
    }

    public sealed class DamageContext
    {
        public DamageContext(DamageRequest request, float rawAmount, bool critical) { Request = request; RawAmount = rawAmount; Critical = critical; }
        public DamageRequest Request { get; }
        public float RawAmount { get; }
        public bool Critical { get; }
    }

    public static class DamageCalculator
    {
        public static DamageResult Apply(DamageRequest request, System.Random random = null)
        {
            if (request == null || request.Target == null) return new DamageResult();
            if (random == null) random = new System.Random();
            var raw = Math.Max(0f, request.Amount + (request.Source?.Stats.GetValue(request.AttackStat) ?? 0f));
            var critical = request.CriticalChance > 0f && random.NextDouble() < request.CriticalChance;
            if (critical) raw *= Math.Max(1f, request.CriticalMultiplier);
            var defense = request.Target.Stats.GetValue(request.DefenseStat);
            var mitigated = Math.Max(0f, raw - Math.Max(0f, defense));
            if (request.MitigationHook != null) mitigated = Math.Max(0f, request.MitigationHook(new DamageContext(request, mitigated, critical)));
            var pool = request.Target.GetResource(request.TargetResource);
            var applied = pool != null ? pool.Consume(mitigated) : mitigated;
            return new DamageResult(raw, mitigated, critical, applied);
        }
    }

    public interface ICombatTargetValidator { bool IsValidTarget(CharacterInstance source, CharacterInstance target, TargetingMode mode, float range); }
    public interface IHitDetector { bool TryGetTarget(object origin, object payload, out CharacterInstance target); }
    public interface ICombatAIHook { void OnCombatEvent(CombatEvent combatEvent); }

    public readonly struct CombatEvent
    {
        public CombatEvent(string eventType, CharacterInstance source, CharacterInstance target, object payload = null) { EventType = eventType; Source = source; Target = target; Payload = payload; }
        public string EventType { get; }
        public CharacterInstance Source { get; }
        public CharacterInstance Target { get; }
        public object Payload { get; }
    }
}
