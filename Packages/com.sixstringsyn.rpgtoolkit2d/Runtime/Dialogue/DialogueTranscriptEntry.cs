using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public enum DialogueTranscriptEntryType { Line, Choice }

    [Serializable]
    public sealed class DialogueTranscriptEntry
    {
        [SerializeField] private DialogueTranscriptEntryType _entryType;
        [SerializeField] private string _nodeId;
        [SerializeField] private string _choiceId;
        [SerializeField] private string _speaker;
        [SerializeField, TextArea] private string _text;
        [SerializeField] private string _localizationKey;
        [SerializeField] private DateTime _timestampUtc;

        public DialogueTranscriptEntry(DialogueTranscriptEntryType entryType, string nodeId, string choiceId, string speaker, string text, string localizationKey, DateTime timestampUtc)
        {
            _entryType = entryType;
            _nodeId = nodeId;
            _choiceId = choiceId;
            _speaker = speaker;
            _text = text;
            _localizationKey = localizationKey;
            _timestampUtc = timestampUtc;
        }

        public DialogueTranscriptEntryType EntryType => _entryType;
        public string NodeId => _nodeId ?? string.Empty;
        public string ChoiceId => _choiceId ?? string.Empty;
        public string Speaker => _speaker ?? string.Empty;
        public string Text => _text ?? string.Empty;
        public string LocalizationKey => _localizationKey ?? string.Empty;
        public DateTime TimestampUtc => _timestampUtc;
    }
}
