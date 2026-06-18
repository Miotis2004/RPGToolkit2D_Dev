using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Cutscenes
{
    public enum RPGCutsceneNodeKind { CameraMove, Dialogue, Animation, Wait, Fade, Branch }

    [Serializable]
    public sealed class RPGCutsceneNode
    {
        public string nodeId;
        public RPGCutsceneNodeKind kind;
        public string targetId;
        public string payloadId;
        public float duration;
        public List<string> nextNodeIds = new List<string>();
    }

    [CreateAssetMenu(fileName = "RPGCutsceneDefinition", menuName = "RPG Toolkit/Cutscenes/Cutscene")]
    public sealed class RPGCutsceneDefinition : RPGObject
    {
        [SerializeField] private string _startNodeId;
        [SerializeField] private bool _skippable = true;
        [SerializeField] private List<RPGCutsceneNode> _nodes = new List<RPGCutsceneNode>();

        public string StartNodeId => _startNodeId;
        public bool Skippable => _skippable;
        public IReadOnlyList<RPGCutsceneNode> Nodes => _nodes;

        public RPGCutsceneNode FindNode(string nodeId) => _nodes.Find(node => node != null && string.Equals(node.nodeId, nodeId, StringComparison.OrdinalIgnoreCase));

        public RPGValidationResult ValidateCutscene()
        {
            var result = new RPGValidationResult();
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in _nodes)
            {
                if (node == null) { result.AddWarning("RPG_CUTSCENE_NULL_NODE", $"{name} contains an empty node.", Id); continue; }
                if (string.IsNullOrWhiteSpace(node.nodeId)) result.AddError("RPG_CUTSCENE_EMPTY_NODE_ID", $"{name} contains a node without an id.", Id);
                else if (!ids.Add(node.nodeId)) result.AddError("RPG_CUTSCENE_DUPLICATE_NODE_ID", $"{name} has duplicate node id {node.nodeId}.", Id);
                if ((node.kind == RPGCutsceneNodeKind.Wait || node.kind == RPGCutsceneNodeKind.Fade || node.kind == RPGCutsceneNodeKind.CameraMove) && node.duration < 0f) result.AddError("RPG_CUTSCENE_NEGATIVE_DURATION", $"{name}/{node.nodeId} has a negative duration.", Id);
            }
            if (!string.IsNullOrWhiteSpace(_startNodeId) && !ids.Contains(_startNodeId)) result.AddError("RPG_CUTSCENE_MISSING_START", $"{name} start node {_startNodeId} does not exist.", Id);
            foreach (var node in _nodes)
            {
                if (node == null) continue;
                foreach (var next in node.nextNodeIds)
                    if (!string.IsNullOrWhiteSpace(next) && !ids.Contains(next)) result.AddError("RPG_CUTSCENE_BROKEN_LINK", $"{name}/{node.nodeId} links to missing node {next}.", Id);
            }
            return result;
        }
    }
}
