using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.World;

namespace SixStringSyn.RPGToolkit2D.Runtime.Party
{
    [Serializable] public sealed class PartySaveData { public List<string> memberIds = new List<string>(); public List<string> activeMemberIds = new List<string>(); public List<CompanionState> companions = new List<CompanionState>(); }
    [Serializable] public sealed class CompanionState { public string characterId; public bool recruited; public int relationship; }
    public sealed class PartyRoster
    {
        private readonly List<CharacterDefinition> _members = new List<CharacterDefinition>(); private readonly List<CharacterDefinition> _active = new List<CharacterDefinition>(); private readonly Dictionary<string, CompanionState> _companions = new Dictionary<string, CompanionState>();
        public event Action Changed; public IReadOnlyList<CharacterDefinition> Members => _members; public IReadOnlyList<CharacterDefinition> ActiveMembers => _active;
        public bool Add(CharacterDefinition character) { if (character == null || _members.Contains(character)) return false; _members.Add(character); Changed?.Invoke(); return true; }
        public bool Remove(CharacterDefinition character) { if (character == null) return false; var removed = _members.Remove(character); _active.Remove(character); if (removed) Changed?.Invoke(); return removed; }
        public bool SetActive(CharacterDefinition character, bool active, int maxActive = 4) { if (character == null || !_members.Contains(character)) return false; if (active) { if (_active.Contains(character)) return true; if (_active.Count >= Math.Max(1, maxActive)) return false; _active.Add(character); } else _active.Remove(character); Changed?.Invoke(); return true; }
        public bool CanRecruit(CharacterDefinition companion, IEnumerable<WorldStateCondition> conditions, WorldState state) { if (companion == null) return false; if (conditions != null) foreach (var condition in conditions) if (condition != null && !condition.Evaluate(state)) return false; return true; }
        public bool Recruit(CharacterDefinition companion, WorldState state = null, IEnumerable<WorldStateCondition> conditions = null) { if (!CanRecruit(companion, conditions, state)) return false; Add(companion); var id = companion.Id.Value; _companions[id] = new CompanionState { characterId = id, recruited = true, relationship = GetRelationship(companion) }; Changed?.Invoke(); return true; }
        public void SetRelationship(CharacterDefinition companion, int value) { if (companion == null) return; var id = companion.Id.Value; if (!_companions.TryGetValue(id, out var state)) _companions[id] = state = new CompanionState { characterId = id }; state.relationship = value; Changed?.Invoke(); }
        public int GetRelationship(CharacterDefinition companion) => companion != null && _companions.TryGetValue(companion.Id.Value, out var state) ? state.relationship : 0;
        public PartySaveData Capture() { var data = new PartySaveData(); foreach (var member in _members) data.memberIds.Add(member.Id.Value); foreach (var member in _active) data.activeMemberIds.Add(member.Id.Value); data.companions.AddRange(_companions.Values); return data; }
        public void Restore(PartySaveData data, Func<string, CharacterDefinition> resolver) { _members.Clear(); _active.Clear(); _companions.Clear(); if (data == null) return; foreach (var id in data.memberIds) { var c = resolver?.Invoke(id); if (c != null) _members.Add(c); } foreach (var id in data.activeMemberIds) { var c = resolver?.Invoke(id); if (c != null && _members.Contains(c)) _active.Add(c); } foreach (var c in data.companions) if (!string.IsNullOrWhiteSpace(c.characterId)) _companions[c.characterId] = c; Changed?.Invoke(); }
    }
}
