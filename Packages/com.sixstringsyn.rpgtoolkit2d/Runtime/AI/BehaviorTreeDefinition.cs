using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Factions;
using SixStringSyn.RPGToolkit2D.Runtime.Schedules;
using SixStringSyn.RPGToolkit2D.Runtime.State;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.AI
{
    public enum BehaviorTreeNodeKind { Selector, Sequence, Action, Condition }
    public enum BehaviorTreeStatus { Inactive, Running, Success, Failure }
    public enum BlackboardValueType { Boolean, Integer, Float, String, Object }

    [Serializable]
    public sealed class BlackboardVariable
    {
        public string key;
        public BlackboardValueType valueType;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;
        public UnityEngine.Object objectValue;
    }

    [Serializable]
    public sealed class BehaviorTreeNode
    {
        public string nodeId;
        public string displayName;
        public BehaviorTreeNodeKind kind;
        public string actionId;
        public string conditionKey;
        public bool expectedBoolValue = true;
        public List<string> children = new List<string>();
        public Rect editorPosition;
    }

    [CreateAssetMenu(fileName = "BehaviorTreeDefinition", menuName = "RPG Toolkit/AI/Behavior Tree")]
    public sealed class BehaviorTreeDefinition : RPGObject
    {
        [SerializeField] private string _rootNodeId;
        [SerializeField] private List<BehaviorTreeNode> _nodes = new List<BehaviorTreeNode>();
        [SerializeField] private List<BlackboardVariable> _blackboardDefaults = new List<BlackboardVariable>();
        [SerializeField] private List<ReputationCondition> _factionConditions = new List<ReputationCondition>();
        [SerializeField] private NPCScheduleDefinition _schedule;

        public string RootNodeId => _rootNodeId;
        public IReadOnlyList<BehaviorTreeNode> Nodes => _nodes;
        public IReadOnlyList<BlackboardVariable> BlackboardDefaults => _blackboardDefaults;
        public IReadOnlyList<ReputationCondition> FactionConditions => _factionConditions;
        public NPCScheduleDefinition Schedule => _schedule;

        public BehaviorTreeNode FindNode(string nodeId) => _nodes.Find(n => n != null && n.nodeId == nodeId);

        public RPGValidationResult ValidateTree()
        {
            var result = new RPGValidationResult();
            if (string.IsNullOrWhiteSpace(_rootNodeId)) result.AddError("RPG_AI_MISSING_ROOT", $"{name} does not define a root behavior node.", Id);
            var ids = new HashSet<string>();
            foreach (var node in _nodes)
            {
                if (node == null) { result.AddWarning("RPG_AI_NULL_NODE", $"{name} contains an empty behavior node.", Id); continue; }
                if (string.IsNullOrWhiteSpace(node.nodeId)) result.AddError("RPG_AI_NODE_MISSING_ID", $"{name} contains a behavior node without an ID.", Id);
                else if (!ids.Add(node.nodeId)) result.AddError("RPG_AI_DUPLICATE_NODE", $"{name} contains duplicate behavior node ID '{node.nodeId}'.", Id);
                if ((node.kind == BehaviorTreeNodeKind.Selector || node.kind == BehaviorTreeNodeKind.Sequence) && (node.children == null || node.children.Count == 0)) result.AddWarning("RPG_AI_COMPOSITE_EMPTY", $"Composite node '{node.nodeId}' has no children.", Id);
                if (node.kind == BehaviorTreeNodeKind.Action && string.IsNullOrWhiteSpace(node.actionId)) result.AddWarning("RPG_AI_ACTION_MISSING_ID", $"Action node '{node.nodeId}' has no action ID.", Id);
                if (node.kind == BehaviorTreeNodeKind.Condition && string.IsNullOrWhiteSpace(node.conditionKey)) result.AddWarning("RPG_AI_CONDITION_MISSING_KEY", $"Condition node '{node.nodeId}' has no blackboard/variable key.", Id);
            }
            if (!string.IsNullOrWhiteSpace(_rootNodeId) && !ids.Contains(_rootNodeId)) result.AddError("RPG_AI_ROOT_NOT_FOUND", $"Root node '{_rootNodeId}' was not found in {name}.", Id);
            foreach (var node in _nodes)
            {
                if (node?.children == null) continue;
                foreach (var child in node.children) if (!string.IsNullOrWhiteSpace(child) && !ids.Contains(child)) result.AddError("RPG_AI_MISSING_CHILD", $"Node '{node.nodeId}' references missing child '{child}'.", Id);
            }
            return result;
        }
    }
}
