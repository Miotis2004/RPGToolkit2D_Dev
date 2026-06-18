using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Factions
{
    [CreateAssetMenu(fileName = "FactionDatabase", menuName = "RPG Toolkit/Factions/Faction Database")]
    public sealed class FactionDatabase : ScriptableObject
    {
        [SerializeField] private List<FactionDefinition> _factions = new List<FactionDefinition>();
        public IReadOnlyList<FactionDefinition> Factions => _factions;
        public FactionDefinition Find(RPGId id) { foreach (var f in _factions) if (f != null && f.Id.Equals(id)) return f; return null; }
        public RPGValidationResult ValidateDatabase() { var r = new RPGValidationResult(); var seen = new HashSet<string>(); foreach (var f in _factions) { if (f == null) { r.AddError("faction.missing", "Faction database contains an empty slot."); continue; } if (!seen.Add(f.Id.Value)) r.AddError("faction.duplicate", $"Duplicate faction id '{f.Id.Value}'.", f.Id); } return r; }
    }
}
