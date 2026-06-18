using System;
using SixStringSyn.RPGToolkit2D.Runtime.Saving;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.World
{
    public sealed class WorldStateSaveContributor : ISaveContributor
    {
        private readonly Func<WorldState> _get; private readonly Action<WorldState> _set;
        public string SystemId => "world_state";
        public WorldStateSaveContributor(Func<WorldState> get, Action<WorldState> set) { _get = get; _set = set; }
        public string CaptureJson() => JsonUtility.ToJson(_get()?.Capture() ?? new WorldStateSaveData());
        public void RestoreJson(string json) { var state = new WorldState(); state.Restore(string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<WorldStateSaveData>(json)); _set?.Invoke(state); }
    }
}
