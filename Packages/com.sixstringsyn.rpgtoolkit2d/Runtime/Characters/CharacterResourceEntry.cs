using System;
using SixStringSyn.RPGToolkit2D.Runtime.Stats;

namespace SixStringSyn.RPGToolkit2D.Runtime.Characters
{
    [Serializable]
    public struct CharacterResourceEntry
    {
        public ResourceDefinition resource;
        public float startingCurrent;
    }
}
