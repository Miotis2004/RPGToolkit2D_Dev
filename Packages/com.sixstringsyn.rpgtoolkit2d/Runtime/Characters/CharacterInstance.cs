using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;

namespace SixStringSyn.RPGToolkit2D.Runtime.Characters
{
    public sealed class CharacterInstance
    {
        private readonly Dictionary<ResourceDefinition, ResourcePool> _resources = new Dictionary<ResourceDefinition, ResourcePool>();

        public CharacterInstance(CharacterDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Level = definition.StartingLevel;
            Experience = definition.ExperienceCurve?.GetRequiredTotalXp(Level) ?? 0;
            Stats = new StatBlock(definition.StatTemplate.Select(entry => new StatValue(entry.stat, entry.value)), definition.Tags);
            Stats.StatChanged += (stat, before, after) => StatChanged?.Invoke(this, stat, before, after);

            foreach (var entry in definition.Resources)
            {
                if (entry.resource == null)
                {
                    continue;
                }

                var maximum = entry.resource.MaximumStat != null ? Stats.GetValue(entry.resource.MaximumStat) : entry.resource.FallbackMaximum;
                var current = entry.startingCurrent > 0f ? entry.startingCurrent : maximum;
                var pool = new ResourcePool(entry.resource, maximum, current);
                pool.Changed += (resource, before, after) => ResourceChanged?.Invoke(this, resource, before, after);
                _resources[entry.resource] = pool;
            }
        }

        public event Action<CharacterInstance, int, int> LevelChanged;
        public event Action<CharacterInstance, StatDefinition, float, float> StatChanged;
        public event Action<CharacterInstance, ResourcePool, float, float> ResourceChanged;

        public CharacterDefinition Definition { get; }
        public StatBlock Stats { get; }
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public IReadOnlyDictionary<ResourceDefinition, ResourcePool> Resources => _resources;

        public ResourcePool GetResource(ResourceDefinition resource) => resource != null && _resources.TryGetValue(resource, out var pool) ? pool : null;

        public void AddExperience(int amount)
        {
            Experience = Math.Max(0, Experience + Math.Max(0, amount));
            var curve = Definition.ExperienceCurve;
            if (curve == null)
            {
                return;
            }

            var before = Level;
            Level = curve.GetLevelForXp(Experience);
            if (before != Level)
            {
                LevelChanged?.Invoke(this, before, Level);
            }
        }
    }
}
