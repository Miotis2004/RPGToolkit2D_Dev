using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogueDefinition", menuName = "RPG Toolkit/Dialogue Definition")]
    public sealed class DialogueDefinition : RPGObject
    {
        [SerializeField] private string _entryNodeId;
        [SerializeField] private List<DialogueNode> _nodes = new List<DialogueNode>();

        public string EntryNodeId => _entryNodeId ?? string.Empty;
        public IReadOnlyList<DialogueNode> Nodes => _nodes;
        public DialogueNode EntryNode => GetNode(_entryNodeId) ?? _nodes.FirstOrDefault(node => node.NodeType == DialogueNodeType.Entry) ?? _nodes.FirstOrDefault();

        public DialogueNode GetNode(string nodeId) => string.IsNullOrWhiteSpace(nodeId) ? null : _nodes.FirstOrDefault(node => node.NodeId == nodeId);

        public DialogueNode AddNode(DialogueNodeType nodeType, string speaker = null, string text = null, Vector2 editorPosition = default)
        {
            var node = new DialogueNode(nodeType, text);
            node.Configure(nodeType, speaker, text);
            node.EditorPosition = editorPosition;
            _nodes.Add(node);
            if (nodeType == DialogueNodeType.Entry || string.IsNullOrWhiteSpace(_entryNodeId)) _entryNodeId = node.NodeId;
            return node;
        }

        public void SetEntryNode(DialogueNode node)
        {
            if (node != null && _nodes.Contains(node)) _entryNodeId = node.NodeId;
        }

        public RPGValidationResult ValidateGraph()
        {
            var result = new RPGValidationResult();
            if (_nodes.Count == 0) { result.AddError("DIALOGUE_EMPTY", "Dialogue contains no nodes.", Id); return result; }
            if (EntryNode == null || (!string.IsNullOrWhiteSpace(_entryNodeId) && GetNode(_entryNodeId) == null)) result.AddError("DIALOGUE_NO_ENTRY", "Dialogue has no valid entry node.", Id);
            var ids = new HashSet<string>();
            foreach (var node in _nodes)
            {
                if (node.NodeType != DialogueNodeType.Exit && string.IsNullOrWhiteSpace(node.Text) && string.IsNullOrWhiteSpace(node.LocalizationKey)) result.AddWarning("DIALOGUE_NODE_TEXT_MISSING", $"Node '{node.NodeId}' has no text or localization key.", Id);
                if (string.IsNullOrWhiteSpace(node.NodeId) || !ids.Add(node.NodeId)) result.AddError("DIALOGUE_DUPLICATE_NODE", "Dialogue contains duplicate or empty node ids.", Id);
                if (!string.IsNullOrWhiteSpace(node.NextNodeId) && GetNode(node.NextNodeId) == null) result.AddError("DIALOGUE_MISSING_NEXT", $"Node '{node.Text}' links to a missing next node.", Id);
                foreach (var choice in node.Choices)
                {
                    if (string.IsNullOrWhiteSpace(choice.Text)) result.AddWarning("DIALOGUE_CHOICE_TEXT_MISSING", $"Node '{node.NodeId}' has a choice with no text.", Id);
                    if (string.IsNullOrWhiteSpace(choice.TargetNodeId) || GetNode(choice.TargetNodeId) == null) result.AddError("DIALOGUE_MISSING_CHOICE_TARGET", $"Choice '{choice.Text}' links to a missing node.", Id);
                }
            }

            var reachable = FindReachableNodeIds();
            foreach (var node in _nodes)
            {
                if (!reachable.Contains(node.NodeId)) result.AddWarning("DIALOGUE_UNREACHABLE_NODE", $"Node '{node.NodeId}' is not reachable from the entry node.", Id);
            }

            return result;
        }

        public IReadOnlyCollection<string> FindReachableNodeIds()
        {
            var reachable = new HashSet<string>();
            var pending = new Queue<DialogueNode>();
            if (EntryNode != null) pending.Enqueue(EntryNode);
            while (pending.Count > 0)
            {
                var node = pending.Dequeue();
                if (node == null || !reachable.Add(node.NodeId)) continue;
                if (!string.IsNullOrWhiteSpace(node.NextNodeId))
                {
                    var next = GetNode(node.NextNodeId);
                    if (next != null) pending.Enqueue(next);
                }

                foreach (var choice in node.Choices)
                {
                    var target = GetNode(choice.TargetNodeId);
                    if (target != null) pending.Enqueue(target);
                }
            }

            return reachable;
        }
    }
}
