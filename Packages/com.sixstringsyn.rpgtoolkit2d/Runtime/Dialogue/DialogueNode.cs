using System;
using System.Collections.Generic;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public enum DialogueNodeType { Line, Entry, Choice, Condition, QuestUpdate, Reward, Exit }

    [Serializable]
    public sealed class DialogueNode
    {
        [SerializeField] private string _nodeId = Guid.NewGuid().ToString("N");
        [SerializeField] private DialogueNodeType _nodeType = DialogueNodeType.Line;
        [SerializeField] private string _speaker;
        [SerializeField, TextArea] private string _text;
        [SerializeField] private string _localizationKey;
        [SerializeField] private Sprite _portrait;
        [SerializeField] private string _speakerAnimation;
        [SerializeField] private string _nextNodeId;
        [SerializeField] private Vector2 _editorPosition;
        [SerializeField] private List<DialogueCondition> _conditions = new List<DialogueCondition>();
        [SerializeField] private List<DialogueCommand> _commands = new List<DialogueCommand>();
        [SerializeField] private List<DialogueChoice> _choices = new List<DialogueChoice>();

        public DialogueNode() { }
        public DialogueNode(DialogueNodeType nodeType, string text = null) { _nodeType = nodeType; _text = text; }

        public string NodeId => _nodeId ?? string.Empty;
        public DialogueNodeType NodeType => _nodeType;
        public string Speaker => _speaker ?? string.Empty;
        public string Text => _text ?? string.Empty;
        public string LocalizationKey => _localizationKey ?? string.Empty;
        public Sprite Portrait => _portrait;
        public string SpeakerAnimation => _speakerAnimation ?? string.Empty;
        public string NextNodeId => _nextNodeId ?? string.Empty;
        public Vector2 EditorPosition { get => _editorPosition; set => _editorPosition = value; }
        public IReadOnlyList<DialogueCondition> Conditions => _conditions;
        public IReadOnlyList<DialogueCommand> Commands => _commands;
        public IReadOnlyList<DialogueChoice> Choices => _choices;

        public void Configure(DialogueNodeType nodeType, string speaker, string text, string nextNodeId = null)
        { _nodeType = nodeType; _speaker = speaker; _text = text; _nextNodeId = nextNodeId; }
        public void SetPresentation(string localizationKey, Sprite portrait = null, string speakerAnimation = null)
        { _localizationKey = localizationKey; _portrait = portrait; _speakerAnimation = speakerAnimation; }
        public void SetNext(string nextNodeId) => _nextNodeId = nextNodeId;
        public void AddChoice(DialogueChoice choice) { if (choice != null) _choices.Add(choice); }
        public void AddCondition(DialogueCondition condition) { if (condition != null) _conditions.Add(condition); }
        public void AddCommand(DialogueCommand command) { if (command != null) _commands.Add(command); }
    }
}
