using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Core
{
    [Serializable]
    public struct RPGId : IEquatable<RPGId>
    {
        [SerializeField]
        private string _value;

        public RPGId(string value)
        {
            _value = Normalize(value);
        }

        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public static RPGId Empty => new RPGId(string.Empty);

        public static RPGId NewId()
        {
            return new RPGId(Guid.NewGuid().ToString("N"));
        }

        public static bool TryParse(string value, out RPGId id)
        {
            id = new RPGId(value);
            return !id.IsEmpty;
        }

        public bool Equals(RPGId other)
        {
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is RPGId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(RPGId left, RPGId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RPGId left, RPGId right)
        {
            return !left.Equals(right);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
