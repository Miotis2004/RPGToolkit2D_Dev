using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Foundation
{
    [CreateAssetMenu(fileName = "RPGContentManifest", menuName = "RPG Toolkit/Foundation/Content Manifest")]
    public sealed class RPGContentManifest : ScriptableObject
    {
        [SerializeField] private RPGSchemaVersion _schemaVersion = RPGSchemaVersion.Initial;
        [SerializeField] private List<RPGObject> _content = new List<RPGObject>();

        public RPGSchemaVersion SchemaVersion => _schemaVersion;
        public IReadOnlyList<RPGObject> Content => _content;

        public void SetContent(IEnumerable<RPGObject> content)
        {
            _content.Clear();
            if (content == null) return;
            _content.AddRange(content);
        }

        public RPGValidationResult Validate()
        {
            var result = new RPGValidationResult();
            var seen = new Dictionary<RPGId, RPGObject>();
            foreach (var entry in _content)
            {
                if (entry == null)
                {
                    result.AddWarning("RPG_MANIFEST_NULL_ENTRY", "Content manifest contains an empty slot.");
                    continue;
                }
                if (entry.Id.IsEmpty)
                {
                    result.AddError("RPG_MANIFEST_EMPTY_ID", $"{entry.name} has no stable RPG id.");
                    continue;
                }
                if (seen.TryGetValue(entry.Id, out var duplicate))
                {
                    result.AddError("RPG_MANIFEST_DUPLICATE_ID", $"{entry.name} and {duplicate.name} share id {entry.Id}.", entry.Id);
                    continue;
                }
                seen.Add(entry.Id, entry);
            }
            return result;
        }
    }
}
