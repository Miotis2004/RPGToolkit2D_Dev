using System;
using System.Collections.Generic;
using System.Linq;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public sealed class DialogueRunner
    {
        private readonly IDialoguePresenter _presenter;
        private DialogueDefinition _definition;
        private IDialogueContext _context;
        private DialogueNode _currentNode;
        private DialogueTranscript _transcript = new DialogueTranscript();

        public DialogueRunner(IDialoguePresenter presenter = null) { _presenter = presenter; }
        public DialogueNode CurrentNode => _currentNode;
        public DialogueTranscript Transcript => _transcript;
        public bool IsRunning => _definition != null && _currentNode != null && _currentNode.NodeType != DialogueNodeType.Exit;
        public event Action<DialogueNode> NodeEntered;
        public event Action<DialogueCommand> CommandExecuted;
        public event Action<DialogueTranscriptEntry> TranscriptEntryAdded;
        public event Action Ended;

        public bool Start(DialogueDefinition definition, IDialogueContext context = null, string entryNodeId = null)
        {
            _definition = definition;
            _context = context;
            _transcript.Clear();
            _currentNode = string.IsNullOrWhiteSpace(entryNodeId) ? definition?.EntryNode : definition?.GetNode(entryNodeId);
            if (_currentNode == null) return false;
            Enter(_currentNode);
            return true;
        }

        public bool Continue()
        {
            if (_currentNode == null) return false;
            if (AvailableChoices.Count > 0) return false;
            return MoveTo(_currentNode.NextNodeId);
        }

        public bool SelectChoice(int index)
        {
            var choices = AvailableChoices;
            if (index < 0 || index >= choices.Count) return false;
            var selectedChoice = choices[index];
            AddTranscriptEntry(_transcript.AddChoice(_currentNode, selectedChoice));
            foreach (var command in selectedChoice.Commands) CommandExecuted?.Invoke(command);
            return MoveTo(selectedChoice.TargetNodeId);
        }

        public IReadOnlyList<DialogueChoice> AvailableChoices => _currentNode == null
            ? Array.Empty<DialogueChoice>()
            : _currentNode.Choices.Where(choice => DialogueConditionEvaluator.AreMet(choice.Conditions, _context)).ToList();

        private bool MoveTo(string nodeId)
        {
            var next = _definition?.GetNode(nodeId);
            if (next == null) { End(); return false; }
            Enter(next);
            return true;
        }

        private void Enter(DialogueNode node)
        {
            _currentNode = node;
            if (!DialogueConditionEvaluator.AreMet(node.Conditions, _context)) { End(); return; }
            foreach (var command in node.Commands) CommandExecuted?.Invoke(command);
            AddTranscriptEntry(_transcript.AddLine(node));
            NodeEntered?.Invoke(node);
            if (node.NodeType == DialogueNodeType.Exit) { End(); return; }
            _presenter?.ShowLine(node, AvailableChoices);
        }

        private void AddTranscriptEntry(DialogueTranscriptEntry entry)
        {
            if (entry != null) TranscriptEntryAdded?.Invoke(entry);
        }

        private void End()
        {
            _presenter?.HideDialogue();
            _definition = null;
            _currentNode = null;
            Ended?.Invoke();
        }
    }
}
