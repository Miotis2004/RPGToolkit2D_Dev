using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;

namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    [Serializable] public sealed class ResourceSaveData { public string resourceId; public float current; public float maximum; }
    [Serializable]
    public sealed class CharacterSaveData
    {
        public string characterId;
        public int level;
        public int experience;
        public List<ResourceSaveData> resources = new List<ResourceSaveData>();

        public static CharacterSaveData FromCharacter(CharacterInstance character)
        {
            var data = new CharacterSaveData();
            if (character == null) return data;
            data.characterId = character.Definition.Id.Value; data.level = character.Level; data.experience = character.Experience;
            foreach (var pool in character.Resources.Values) data.resources.Add(new ResourceSaveData { resourceId = pool.Definition.Id.Value, current = pool.Current, maximum = pool.Maximum });
            return data;
        }

        public CharacterInstance ToCharacter(Func<RPGId, CharacterDefinition> resolver)
        {
            var definition = resolver?.Invoke(new RPGId(characterId));
            if (definition == null) return null;
            var character = new CharacterInstance(definition);
            character.AddExperience(Math.Max(0, experience - character.Experience));
            foreach (var saved in resources)
            {
                foreach (var pool in character.Resources.Values)
                {
                    if (pool.Definition.Id == new RPGId(saved.resourceId))
                    {
                        pool.SetMaximum(saved.maximum);
                        if (pool.Current > saved.current) pool.Consume(pool.Current - saved.current); else pool.Restore(saved.current - pool.Current);
                    }
                }
            }
            return character;
        }
    }
}
