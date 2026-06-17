using System.Collections.Generic;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Core
{
    public abstract class RPGObject : ScriptableObject
    {
        [SerializeField]
        private RPGId _id = RPGId.NewId();

        [SerializeField]
        private string _displayName;

        [SerializeField, TextArea]
        private string _description;

        [SerializeField]
        private List<RPGTag> _tags = new List<RPGTag>();

        public RPGId Id => _id;
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
        public string Description => _description ?? string.Empty;
        public IReadOnlyList<RPGTag> Tags => _tags;

        public void AssignNewId()
        {
            _id = RPGId.NewId();
        }

        public void SetId(RPGId id)
        {
            _id = id;
        }

        public bool HasTag(RPGTag tag) => RPGTagQuery.HasTag(_tags, tag);

        protected virtual void OnValidate()
        {
            if (_id.IsEmpty)
            {
                _id = RPGId.NewId();
            }
        }
    }
}
