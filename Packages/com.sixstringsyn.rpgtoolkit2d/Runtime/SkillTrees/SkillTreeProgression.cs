using System.Collections.Generic;

namespace SixStringSyn.RPGToolkit2D.Runtime.SkillTrees
{
    public sealed class SkillTreeProgression
    {
        private readonly HashSet<string> _unlocked = new HashSet<string>();
        public IReadOnlyCollection<string> UnlockedNodeIds => _unlocked;
        public bool IsUnlocked(string nodeId) => !string.IsNullOrWhiteSpace(nodeId) && _unlocked.Contains(nodeId);
        public bool CanUnlock(SkillTreeDefinition tree, string nodeId, int availablePoints, out string reason)
        {
            reason = string.Empty; var node = tree == null ? null : tree.FindNode(nodeId); if (node == null) { reason = "Skill node was not found."; return false; } if (IsUnlocked(nodeId)) { reason = "Skill node is already unlocked."; return false; } if (availablePoints < node.pointCost) { reason = "Not enough skill points."; return false; } foreach (var prerequisite in node.prerequisites) if (!IsUnlocked(prerequisite)) { reason = $"Missing prerequisite '{prerequisite}'."; return false; } return true;
        }
        public bool Unlock(SkillTreeDefinition tree, string nodeId, ref int availablePoints, out string reason) { if (!CanUnlock(tree, nodeId, availablePoints, out reason)) return false; var node = tree.FindNode(nodeId); availablePoints -= node.pointCost; _unlocked.Add(nodeId); return true; }
    }
}
