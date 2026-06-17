using UnityEngine;
using SixStringSyn.RPGToolkit2D.Runtime.Core;

namespace SixStringSyn.RPGToolkit2D.Runtime.Characters
{
    [CreateAssetMenu(fileName = "NewExperienceCurve", menuName = "RPG Toolkit/Characters/Experience Curve")]
    public sealed class ExperienceCurveDefinition : RPGObject
    {
        [SerializeField]
        private int[] _xpRequiredPerLevel = { 0, 100, 300, 600, 1000 };

        public int MaxLevel => _xpRequiredPerLevel == null ? 1 : Mathf.Max(1, _xpRequiredPerLevel.Length);

        public int GetRequiredTotalXp(int level)
        {
            if (_xpRequiredPerLevel == null || _xpRequiredPerLevel.Length == 0)
            {
                return 0;
            }

            return _xpRequiredPerLevel[Mathf.Clamp(level - 1, 0, _xpRequiredPerLevel.Length - 1)];
        }

        public int GetLevelForXp(int totalXp)
        {
            var level = 1;
            for (var i = 0; i < MaxLevel; i++)
            {
                if (totalXp >= GetRequiredTotalXp(i + 1))
                {
                    level = i + 1;
                }
            }

            return level;
        }
    }
}
