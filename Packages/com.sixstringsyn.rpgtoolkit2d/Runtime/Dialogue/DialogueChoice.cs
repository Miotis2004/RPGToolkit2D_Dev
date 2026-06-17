using System;
using System.Collections.Generic;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    [Serializable]
    public sealed class DialogueChoice
    {
        [SerializeField] private string _choiceId = Guid.NewGuid().ToString("N");
        [SerializeField] private string _text;
        [SerializeField] private string _targetNodeId;
        [SerializeField] private List<DialogueCondition> _conditions = new List<DialogueCondition>();
        [SerializeField] private List<DialogueCommand> _commands = new List<DialogueCommand>();

        public DialogueChoice() { }

        public DialogueChoice(string text, string targetNodeId)
        {
            _text = text;
            _targetNodeId = targetNodeId;
        }

        public string ChoiceId => _choiceId ?? string.Empty;
        public string Text => _text ?? string.Empty;
        public string TargetNodeId => _targetNodeId ?? string.Empty;
        public IReadOnlyList<DialogueCondition> Conditions => _conditions;
        public IReadOnlyList<DialogueCommand> Commands => _commands;

        public void SetTarget(string targetNodeId) => _targetNodeId = targetNodeId;
        public void SetText(string text) => _text = text;
        public void AddCondition(DialogueCondition condition) { if (condition != null) _conditions.Add(condition); }
        public void AddCommand(DialogueCommand command) { if (command != null) _commands.Add(command); }
    }
}
