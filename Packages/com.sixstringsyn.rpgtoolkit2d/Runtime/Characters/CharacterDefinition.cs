using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Characters
{
    [CreateAssetMenu(fileName = "NewCharacterDefinition", menuName = "RPG Toolkit/Character Definition")]
    public sealed class CharacterDefinition : RPGObject
    {
        [SerializeField]
        private int _startingLevel = 1;

        [SerializeField]
        private Sprite _portrait;

        [SerializeField]
        private GameObject _prefab;

        [SerializeField]
        private ExperienceCurveDefinition _experienceCurve;

        [SerializeField]
        private List<CharacterStatEntry> _statTemplate = new List<CharacterStatEntry>();

        [SerializeField]
        private List<CharacterResourceEntry> _resources = new List<CharacterResourceEntry>();

        public int StartingLevel => Mathf.Max(1, _startingLevel);
        public Sprite Portrait => _portrait;
        public GameObject Prefab => _prefab;
        public ExperienceCurveDefinition ExperienceCurve => _experienceCurve;
        public IReadOnlyList<CharacterStatEntry> StatTemplate => _statTemplate;
        public IReadOnlyList<CharacterResourceEntry> Resources => _resources;

        protected override void OnValidate()
        {
            base.OnValidate();
            _startingLevel = Mathf.Max(1, _startingLevel);
        }
    }
}
