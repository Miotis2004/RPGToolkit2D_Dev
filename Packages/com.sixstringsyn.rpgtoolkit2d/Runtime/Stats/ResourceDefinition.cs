using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Stats
{
    [CreateAssetMenu(fileName = "NewResourceDefinition", menuName = "RPG Toolkit/Stats/Resource Definition")]
    public sealed class ResourceDefinition : RPGObject
    {
        [SerializeField]
        private StatDefinition _maximumStat;

        [SerializeField]
        private float _fallbackMaximum = 100f;

        [SerializeField]
        private float _regenerationPerSecond;

        public StatDefinition MaximumStat => _maximumStat;
        public float FallbackMaximum => Mathf.Max(0f, _fallbackMaximum);
        public float RegenerationPerSecond => _regenerationPerSecond;
    }
}
