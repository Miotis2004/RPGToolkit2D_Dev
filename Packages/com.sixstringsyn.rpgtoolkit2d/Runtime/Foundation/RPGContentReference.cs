using System;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Foundation
{
    [Serializable]
    public struct RPGContentReference : IEquatable<RPGContentReference>
    {
        [SerializeField] private RPGContentKind _kind;
        [SerializeField] private RPGId _id;

        public RPGContentReference(RPGContentKind kind, RPGId id)
        {
            _kind = kind;
            _id = id;
        }

        public RPGContentKind Kind => _kind;
        public RPGId Id => _id;
        public bool IsEmpty => _kind == RPGContentKind.Unknown || _id.IsEmpty;

        public bool Equals(RPGContentReference other) => _kind == other._kind && _id.Equals(other._id);
        public override bool Equals(object obj) => obj is RPGContentReference other && Equals(other);
        public override int GetHashCode() => ((int)_kind * 397) ^ _id.GetHashCode();
        public override string ToString() => IsEmpty ? string.Empty : $"{_kind}:{_id}";
    }
}
