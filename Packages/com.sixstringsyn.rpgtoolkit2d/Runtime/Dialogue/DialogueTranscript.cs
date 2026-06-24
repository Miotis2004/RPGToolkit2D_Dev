using System;
using System.Collections.Generic;
using System.Linq;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public sealed class DialogueTranscript
    {
        private readonly List<DialogueTranscriptEntry> _entries = new List<DialogueTranscriptEntry>();

        public IReadOnlyList<DialogueTranscriptEntry> Entries => _entries;
        public int Count => _entries.Count;

        public DialogueTranscriptEntry AddLine(DialogueNode node) => AddLine(node, DateTime.UtcNow);

        public DialogueTranscriptEntry AddLine(DialogueNode node, DateTime timestampUtc)
        {
            if (node == null) return null;
            var entry = new DialogueTranscriptEntry(DialogueTranscriptEntryType.Line, node.NodeId, string.Empty, node.Speaker, node.Text, node.LocalizationKey, timestampUtc);
            _entries.Add(entry);
            return entry;
        }

        public DialogueTranscriptEntry AddChoice(DialogueNode node, DialogueChoice choice) => AddChoice(node, choice, DateTime.UtcNow);

        public DialogueTranscriptEntry AddChoice(DialogueNode node, DialogueChoice choice, DateTime timestampUtc)
        {
            if (choice == null) return null;
            var entry = new DialogueTranscriptEntry(DialogueTranscriptEntryType.Choice, node?.NodeId, choice.ChoiceId, string.Empty, choice.Text, string.Empty, timestampUtc);
            _entries.Add(entry);
            return entry;
        }

        public IReadOnlyList<DialogueTranscriptEntry> GetEntriesForSpeaker(string speaker)
        {
            if (string.IsNullOrWhiteSpace(speaker)) return Array.Empty<DialogueTranscriptEntry>();
            return _entries.Where(entry => string.Equals(entry.Speaker, speaker, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public void Clear() => _entries.Clear();
    }
}
