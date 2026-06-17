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

        public DialogueRunner(IDialoguePresenter presenter = null) { _presenter = presenter; }
        public DialogueNode CurrentNode => _currentNode;
        public bool IsRunning => _definition != null && _currentNode != null && _currentNode.NodeType != DialogueNodeType.Exit;
        public event Action<DialogueNode> NodeEntered;
        public event Action<DialogueCommand> CommandExecuted;
        public event Action Ended;

        public bool Start(DialogueDefinition definition, IDialogueContext context = null, string entryNodeId = null)
        {
            _definition = definition;
            _context = context;
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
            foreach (var command in choices[index].Commands) CommandExecuted?.Invoke(command);
            return MoveTo(choices[index].TargetNodeId);
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
            NodeEntered?.Invoke(node);
            if (node.NodeType == DialogueNodeType.Exit) { End(); return; }
            _presenter?.ShowLine(node, AvailableChoices);
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
