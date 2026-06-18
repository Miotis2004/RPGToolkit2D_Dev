using System;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Saving;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Party
{
    public sealed class PartySaveContributor : ISaveContributor
    {
        private readonly Func<PartyRoster> _get; private readonly Action<PartyRoster> _set; private readonly Func<string, CharacterDefinition> _resolve;
        public string SystemId => "party";
        public PartySaveContributor(Func<PartyRoster> get, Action<PartyRoster> set, Func<string, CharacterDefinition> resolve) { _get = get; _set = set; _resolve = resolve; }
        public string CaptureJson() => JsonUtility.ToJson(_get()?.Capture() ?? new PartySaveData());
        public void RestoreJson(string json) { var roster = new PartyRoster(); roster.Restore(string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<PartySaveData>(json), _resolve); _set?.Invoke(roster); }
    }
}
