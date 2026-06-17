using System.Collections.Generic;

namespace SixStringSyn.RPGToolkit2D.Runtime.Core
{
    public sealed class RPGDatabase<T> where T : RPGObject
    {
        private readonly List<T> _definitions = new List<T>();
        private readonly Dictionary<RPGId, T> _byId = new Dictionary<RPGId, T>();

        public RPGDatabase(IEnumerable<T> definitions)
        {
            if (definitions == null) return;
            foreach (var definition in definitions)
            {
                if (definition != null)
                {
                    _definitions.Add(definition);
                    if (!definition.Id.IsEmpty && !_byId.ContainsKey(definition.Id))
                    {
                        _byId.Add(definition.Id, definition);
                    }
                }
            }
        }

        public IReadOnlyList<T> Definitions => _definitions;

        public bool TryGet(RPGId id, out T definition) => _byId.TryGetValue(id, out definition);

        public T GetOrDefault(RPGId id)
        {
            TryGet(id, out var definition);
            return definition;
        }

        public RPGValidationResult Validate()
        {
            var result = new RPGValidationResult();
            var seen = new Dictionary<RPGId, T>();

            foreach (var definition in _definitions)
            {
                if (definition.Id.IsEmpty)
                {
                    result.AddError("RPGDB_EMPTY_ID", $"{definition.name} has an empty RPG id.");
                    continue;
                }

                if (seen.TryGetValue(definition.Id, out var duplicate))
                {
                    result.AddError("RPGDB_DUPLICATE_ID", $"{definition.name} and {duplicate.name} share id {definition.Id}.", definition.Id);
                    continue;
                }

                seen.Add(definition.Id, definition);
            }

            return result;
        }
    }
}
