using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public enum DialogueConditionOperator
    {
        Exists,
        NotExists,
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }

    [Serializable]
    public sealed class DialogueCondition
    {
        [SerializeField] private string _key;
        [SerializeField] private DialogueConditionOperator _operator = DialogueConditionOperator.Exists;
        [SerializeField] private string _value;

        public DialogueCondition() { }

        public DialogueCondition(string key, DialogueConditionOperator conditionOperator, string value = null)
        {
            _key = key;
            _operator = conditionOperator;
            _value = value;
        }

        public string Key => _key ?? string.Empty;
        public DialogueConditionOperator Operator => _operator;
        public string Value => _value ?? string.Empty;
    }
}
