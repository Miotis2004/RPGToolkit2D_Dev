using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Schedules
{
    [Serializable]
    public struct RPGTimeOfDay : IComparable<RPGTimeOfDay>
    {
        [Range(0, 23)] public int hour;
        [Range(0, 59)] public int minute;
        public int TotalMinutes => Mathf.Clamp(hour, 0, 23) * 60 + Mathf.Clamp(minute, 0, 59);
        public int CompareTo(RPGTimeOfDay other) => TotalMinutes.CompareTo(other.TotalMinutes);
        public override string ToString() => $"{Mathf.Clamp(hour, 0, 23):00}:{Mathf.Clamp(minute, 0, 59):00}";
    }

    [Serializable]
    public sealed class NPCScheduleEntry
    {
        public RPGTimeOfDay time;
        public string destinationId;
        public string actionId;
        public string eventHookId;
    }

    [CreateAssetMenu(fileName = "NPCScheduleDefinition", menuName = "RPG Toolkit/World/NPC Schedule")]
    public sealed class NPCScheduleDefinition : RPGObject
    {
        [SerializeField] private List<NPCScheduleEntry> _entries = new List<NPCScheduleEntry>();
        public IReadOnlyList<NPCScheduleEntry> Entries => _entries;

        public NPCScheduleEntry Resolve(RPGTimeOfDay time)
        {
            NPCScheduleEntry best = null;
            var bestMinutes = -1;
            foreach (var entry in _entries)
            {
                if (entry == null) continue;
                var minutes = entry.time.TotalMinutes;
                if (minutes <= time.TotalMinutes && minutes >= bestMinutes) { best = entry; bestMinutes = minutes; }
            }
            if (best != null) return best;
            foreach (var entry in _entries)
            {
                if (entry == null) continue;
                var minutes = entry.time.TotalMinutes;
                if (minutes >= bestMinutes) { best = entry; bestMinutes = minutes; }
            }
            return best;
        }

        public RPGValidationResult ValidateSchedule()
        {
            var result = new RPGValidationResult();
            var times = new HashSet<int>();
            foreach (var entry in _entries)
            {
                if (entry == null) { result.AddWarning("RPG_SCHEDULE_NULL_ENTRY", $"{name} contains an empty schedule entry.", Id); continue; }
                if (!times.Add(entry.time.TotalMinutes)) result.AddWarning("RPG_SCHEDULE_DUPLICATE_TIME", $"{name} has multiple entries at {entry.time}.", Id);
                if (string.IsNullOrWhiteSpace(entry.destinationId) && string.IsNullOrWhiteSpace(entry.actionId)) result.AddWarning("RPG_SCHEDULE_EMPTY_ENTRY", $"{name} has no destination or action at {entry.time}.", Id);
            }
            return result;
        }
    }
}
