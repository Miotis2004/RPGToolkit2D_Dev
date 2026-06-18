using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Abilities;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.SkillTrees
{
    [System.Serializable] public sealed class SkillTreeNode { public string nodeId; public AbilityDefinition ability; public int pointCost = 1; public List<string> prerequisites = new List<string>(); public Vector2 editorPosition; }
    [CreateAssetMenu(fileName = "NewSkillTree", menuName = "RPG Toolkit/Skill Trees/Skill Tree")]
    public sealed class SkillTreeDefinition : RPGObject
    {
        [SerializeField] private List<SkillTreeNode> _nodes = new List<SkillTreeNode>();
        public IReadOnlyList<SkillTreeNode> Nodes => _nodes;
        public SkillTreeNode FindNode(string nodeId) { foreach (var n in _nodes) if (n != null && n.nodeId == nodeId) return n; return null; }
        public RPGValidationResult ValidateTree() { var r = new RPGValidationResult(); var ids = new HashSet<string>(); foreach (var n in _nodes) { if (n == null || string.IsNullOrWhiteSpace(n.nodeId)) { r.AddError("skill.node.id", "Skill tree contains a node without an id.", Id); continue; } if (!ids.Add(n.nodeId)) r.AddError("skill.node.duplicate", $"Duplicate skill node '{n.nodeId}'.", Id); if (n.pointCost < 0) r.AddError("skill.node.cost", $"Skill node '{n.nodeId}' has a negative point cost.", Id); } foreach (var n in _nodes) if (n != null) foreach (var p in n.prerequisites) if (!string.IsNullOrWhiteSpace(p) && !ids.Contains(p)) r.AddError("skill.node.prerequisite", $"Skill node '{n.nodeId}' references missing prerequisite '{p}'.", Id); return r; }
        protected override void OnValidate() { base.OnValidate(); foreach (var n in _nodes) if (n != null) n.pointCost = Mathf.Max(0, n.pointCost); }
    }
}
