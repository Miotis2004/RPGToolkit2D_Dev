using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.World
{
    public sealed class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnPointId = "default";
        public string SpawnPointId => _spawnPointId;
    }
}
