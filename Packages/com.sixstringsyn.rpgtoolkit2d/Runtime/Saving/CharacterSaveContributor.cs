using System;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    public sealed class CharacterSaveContributor : ISaveContributor
    {
        private readonly Func<CharacterInstance> _getter;
        private readonly Action<CharacterInstance> _setter;
        private readonly Func<RPGId, CharacterDefinition> _resolver;
        public CharacterSaveContributor(Func<CharacterInstance> getter, Action<CharacterInstance> setter, Func<RPGId, CharacterDefinition> resolver, string systemId = "character")
        { _getter = getter; _setter = setter; _resolver = resolver; SystemId = systemId; }
        public string SystemId { get; }
        public string CaptureJson() => JsonUtility.ToJson(CharacterSaveData.FromCharacter(_getter?.Invoke()));
        public void RestoreJson(string json) => _setter?.Invoke(JsonUtility.FromJson<CharacterSaveData>(json).ToCharacter(_resolver));
    }
}
