using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Foundation
{
    [Serializable]
    public struct RPGSchemaVersion : IComparable<RPGSchemaVersion>, IEquatable<RPGSchemaVersion>
    {
        [SerializeField] private int _major;
        [SerializeField] private int _minor;
        [SerializeField] private int _patch;

        public RPGSchemaVersion(int major, int minor = 0, int patch = 0)
        {
            _major = Math.Max(0, major);
            _minor = Math.Max(0, minor);
            _patch = Math.Max(0, patch);
        }

        public int Major => _major;
        public int Minor => _minor;
        public int Patch => _patch;
        public static RPGSchemaVersion Initial => new RPGSchemaVersion(1, 0, 0);
        public bool IsCompatibleWith(RPGSchemaVersion savedVersion) => _major == savedVersion._major;
        public int CompareTo(RPGSchemaVersion other)
        {
            var major = _major.CompareTo(other._major);
            if (major != 0) return major;
            var minor = _minor.CompareTo(other._minor);
            return minor != 0 ? minor : _patch.CompareTo(other._patch);
        }
        public bool Equals(RPGSchemaVersion other) => _major == other._major && _minor == other._minor && _patch == other._patch;
        public override bool Equals(object obj) => obj is RPGSchemaVersion other && Equals(other);
        public override int GetHashCode() => (_major * 397) ^ (_minor * 31) ^ _patch;
        public override string ToString() => $"{_major}.{_minor}.{_patch}";
    }
}
