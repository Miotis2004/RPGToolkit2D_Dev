using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Stats
{
    [CreateAssetMenu(fileName = "NewStatDefinition", menuName = "RPG Toolkit/Stats/Stat Definition")]
    public sealed class StatDefinition : RPGObject
    {
        [SerializeField]
        private float _defaultValue;

        [SerializeField]
        private float _minimumValue;

        [SerializeField]
        private float _maximumValue = 9999f;

        public float DefaultValue => _defaultValue;
        public float MinimumValue => _minimumValue;
        public float MaximumValue => _maximumValue;

        public float Clamp(float value) => Mathf.Clamp(value, _minimumValue, _maximumValue);
    }
}
