using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.World
{
    [Serializable]
    public sealed class SceneTransitionRequest
    {
        [SerializeField] private string _targetSceneName;
        [SerializeField] private string _spawnPointId;
        [SerializeField] private bool _saveBeforeTransition;
        public string TargetSceneName => _targetSceneName;
        public string SpawnPointId => _spawnPointId;
        public bool SaveBeforeTransition => _saveBeforeTransition;
        public SceneTransitionRequest(string targetSceneName, string spawnPointId = null, bool saveBeforeTransition = false) { _targetSceneName = targetSceneName; _spawnPointId = spawnPointId; _saveBeforeTransition = saveBeforeTransition; }
        public bool IsValid => !string.IsNullOrWhiteSpace(_targetSceneName);
    }
}
