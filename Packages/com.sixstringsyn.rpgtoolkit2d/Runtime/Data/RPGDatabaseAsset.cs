using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Data
{
    [Serializable]
    public sealed class RPGDatabaseReference
    {
        [SerializeField] private RPGContentKind _expectedKind = RPGContentKind.Unknown;
        [SerializeField] private RPGId _id;
        public RPGContentKind ExpectedKind => _expectedKind;
        public RPGId Id => _id;
        public bool IsEmpty => _id.IsEmpty;
        public RPGDatabaseReference(RPGContentKind expectedKind, RPGId id) { _expectedKind = expectedKind; _id = id; }
    }

    public enum RPGDatabaseSortMode { AssetName, DisplayName, Id }

    [CreateAssetMenu(fileName = "RPGContentDatabase", menuName = "RPG Toolkit/Data/Content Database")]
    public sealed class RPGDatabaseAsset : ScriptableObject
    {
        [SerializeField] private RPGContentKind _contentKind = RPGContentKind.Unknown;
        [SerializeField] private List<RPGObject> _records = new List<RPGObject>();

        public RPGContentKind ContentKind => _contentKind;
        public IReadOnlyList<RPGObject> Records => _records;
        public RPGDatabase<RPGObject> RuntimeDatabase => new RPGDatabase<RPGObject>(_records);

        public void SetRecords(IEnumerable<RPGObject> records) { _records.Clear(); if (records != null) _records.AddRange(records); }

        public bool TryGet(RPGId id, out RPGObject record) => RuntimeDatabase.TryGet(id, out record);
        public RPGObject Resolve(RPGDatabaseReference reference) => reference == null || reference.IsEmpty ? null : RuntimeDatabase.GetOrDefault(reference.Id);

        public List<RPGObject> Search(string query = null, IEnumerable<RPGTag> requiredTags = null, RPGDatabaseSortMode sortMode = RPGDatabaseSortMode.DisplayName)
        {
            var results = new List<RPGObject>();
            var tags = requiredTags == null ? new List<RPGTag>() : new List<RPGTag>(requiredTags);
            foreach (var record in _records)
            {
                if (record == null) continue;
                if (!MatchesQuery(record, query)) continue;
                var hasAllTags = true;
                foreach (var tag in tags) if (!record.HasTag(tag)) { hasAllTags = false; break; }
                if (hasAllTags) results.Add(record);
            }
            results.Sort((a, b) => string.Compare(GetSortKey(a, sortMode), GetSortKey(b, sortMode), StringComparison.OrdinalIgnoreCase));
            return results;
        }

        public RPGValidationResult ValidateReferences(IEnumerable<RPGDatabaseReference> references)
        {
            var result = Validate();
            if (references == null) return result;
            var runtime = RuntimeDatabase;
            foreach (var reference in references)
            {
                if (reference == null || reference.IsEmpty) continue;
                if (reference.ExpectedKind != RPGContentKind.Unknown && _contentKind != RPGContentKind.Unknown && reference.ExpectedKind != _contentKind)
                    result.AddError("RPGDB_REFERENCE_KIND_MISMATCH", $"Reference {reference.Id} expects {reference.ExpectedKind} but database contains {_contentKind}.", reference.Id);
                if (!runtime.TryGet(reference.Id, out _)) result.AddError("RPGDB_BROKEN_REFERENCE", $"Reference {reference.Id} does not resolve in {name}.", reference.Id);
            }
            return result;
        }

        public RPGValidationResult Validate()
        {
            var result = RuntimeDatabase.Validate();
            foreach (var record in _records) if (record == null) result.AddWarning("RPGDB_NULL_RECORD", $"{name} contains an empty record slot.");
            return result;
        }

        private static bool MatchesQuery(RPGObject record, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            var q = query.Trim();
            return record.name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   record.DisplayName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   record.Description.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   record.Id.ToString().IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetSortKey(RPGObject record, RPGDatabaseSortMode sortMode)
        {
            switch (sortMode)
            {
                case RPGDatabaseSortMode.AssetName: return record.name;
                case RPGDatabaseSortMode.Id: return record.Id.ToString();
                default: return record.DisplayName;
            }
        }
    }
}
