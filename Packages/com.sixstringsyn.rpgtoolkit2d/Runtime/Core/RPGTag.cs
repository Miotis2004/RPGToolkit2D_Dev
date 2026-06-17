using System;
using System.Collections.Generic;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Core
{
    [Serializable]
    public struct RPGTag : IEquatable<RPGTag>
    {
        [SerializeField]
        private string _value;

        public RPGTag(string value)
        {
            _value = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }

        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(RPGTag other) => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is RPGTag other && Equals(other);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public override string ToString() => Value;

        public static bool operator ==(RPGTag left, RPGTag right) => left.Equals(right);

        public static bool operator !=(RPGTag left, RPGTag right) => !left.Equals(right);
    }

    public static class RPGTagQuery
    {
        public static bool HasTag(IEnumerable<RPGTag> tags, RPGTag requiredTag)
        {
            if (requiredTag.IsEmpty || tags == null) return false;
            foreach (var tag in tags)
            {
                if (tag == requiredTag) return true;
            }
            return false;
        }

        public static bool HasAll(IEnumerable<RPGTag> tags, IEnumerable<RPGTag> requiredTags)
        {
            var set = ToSet(tags);
            foreach (var tag in requiredTags ?? Array.Empty<RPGTag>())
            {
                if (!tag.IsEmpty && !set.Contains(tag)) return false;
            }
            return true;
        }

        public static bool HasAny(IEnumerable<RPGTag> tags, IEnumerable<RPGTag> candidateTags)
        {
            var set = ToSet(tags);
            foreach (var tag in candidateTags ?? Array.Empty<RPGTag>())
            {
                if (!tag.IsEmpty && set.Contains(tag)) return true;
            }
            return false;
        }

        private static HashSet<RPGTag> ToSet(IEnumerable<RPGTag> tags)
        {
            return tags == null ? new HashSet<RPGTag>() : new HashSet<RPGTag>(tags);
        }
    }
}
