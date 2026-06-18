using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.State;

namespace SixStringSyn.RPGToolkit2D.Runtime.AI
{
    public sealed class BehaviorTreeBlackboard
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        public RPGStateStore WorldState { get; set; }
        public IReadOnlyDictionary<string, object> Values => _values;
        public void Set(string key, object value) { if (!string.IsNullOrWhiteSpace(key)) _values[key] = value; }
        public T Get<T>(string key, T fallback = default) => !string.IsNullOrWhiteSpace(key) && _values.TryGetValue(key, out var value) && value is T typed ? typed : fallback;
        public bool EvaluateBool(string key, bool expected) => Get(key, WorldState?.GetSwitch(key) ?? false) == expected;
    }

    public sealed class BehaviorTreeTickResult
    {
        public BehaviorTreeStatus Status { get; }
        public IReadOnlyList<string> VisitedNodeIds { get; }
        public BehaviorTreeTickResult(BehaviorTreeStatus status, List<string> visited) { Status = status; VisitedNodeIds = visited; }
    }

    public sealed class BehaviorTreeExecutor
    {
        private readonly Dictionary<string, Func<BehaviorTreeBlackboard, BehaviorTreeStatus>> _actions = new Dictionary<string, Func<BehaviorTreeBlackboard, BehaviorTreeStatus>>();
        public event Action<string, BehaviorTreeStatus> NodeEvaluated;
        public void RegisterAction(string actionId, Func<BehaviorTreeBlackboard, BehaviorTreeStatus> action) { if (!string.IsNullOrWhiteSpace(actionId) && action != null) _actions[actionId] = action; }
        public BehaviorTreeTickResult Tick(BehaviorTreeDefinition tree, BehaviorTreeBlackboard blackboard)
        {
            var visited = new List<string>();
            if (tree == null || blackboard == null || string.IsNullOrWhiteSpace(tree.RootNodeId)) return new BehaviorTreeTickResult(BehaviorTreeStatus.Failure, visited);
            foreach (var variable in tree.BlackboardDefaults) ApplyDefault(blackboard, variable);
            var status = Evaluate(tree, tree.RootNodeId, blackboard, visited, new HashSet<string>());
            return new BehaviorTreeTickResult(status, visited);
        }
        private BehaviorTreeStatus Evaluate(BehaviorTreeDefinition tree, string nodeId, BehaviorTreeBlackboard blackboard, List<string> visited, HashSet<string> stack)
        {
            var node = tree.FindNode(nodeId); if (node == null || !stack.Add(nodeId)) return BehaviorTreeStatus.Failure; visited.Add(nodeId);
            BehaviorTreeStatus status;
            if (node.kind == BehaviorTreeNodeKind.Selector) { status = BehaviorTreeStatus.Failure; foreach (var child in node.children) { var childStatus = Evaluate(tree, child, blackboard, visited, stack); if (childStatus != BehaviorTreeStatus.Failure) { status = childStatus; break; } } }
            else if (node.kind == BehaviorTreeNodeKind.Sequence) { status = BehaviorTreeStatus.Success; foreach (var child in node.children) { var childStatus = Evaluate(tree, child, blackboard, visited, stack); if (childStatus != BehaviorTreeStatus.Success) { status = childStatus; break; } } }
            else if (node.kind == BehaviorTreeNodeKind.Condition) status = blackboard.EvaluateBool(node.conditionKey, node.expectedBoolValue) ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Failure;
            else status = _actions.TryGetValue(node.actionId, out var action) ? action(blackboard) : BehaviorTreeStatus.Failure;
            stack.Remove(nodeId); NodeEvaluated?.Invoke(nodeId, status); return status;
        }
        private static void ApplyDefault(BehaviorTreeBlackboard blackboard, BlackboardVariable variable)
        {
            if (variable == null || string.IsNullOrWhiteSpace(variable.key) || blackboard.Values.ContainsKey(variable.key)) return;
            if (variable.valueType == BlackboardValueType.Boolean) blackboard.Set(variable.key, variable.boolValue); else if (variable.valueType == BlackboardValueType.Integer) blackboard.Set(variable.key, variable.intValue); else if (variable.valueType == BlackboardValueType.Float) blackboard.Set(variable.key, variable.floatValue); else if (variable.valueType == BlackboardValueType.String) blackboard.Set(variable.key, variable.stringValue); else blackboard.Set(variable.key, variable.objectValue);
        }
    }
}
