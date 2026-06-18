using SixStringSyn.RPGToolkit2D.Runtime.AI;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.AI
{
    public sealed class BehaviorTreeEditorWindow : EditorWindow
    {
        private BehaviorTreeDefinition _tree;
        private UnityEditor.Editor _editor;
        private Vector2 _scroll;

        [MenuItem("Tools/RPG Toolkit/AI/Behavior Tree Editor")]
        public static void Open() => GetWindow<BehaviorTreeEditorWindow>("Behavior Trees");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("AI Behavior Tree Editor", EditorStyles.boldLabel);
            _tree = (BehaviorTreeDefinition)EditorGUILayout.ObjectField("Behavior Tree", _tree, typeof(BehaviorTreeDefinition), false);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Behavior Tree")) ProjectWindowUtil.CreateAsset(CreateInstance<BehaviorTreeDefinition>(), "NewBehaviorTree.asset");
            using (new EditorGUI.DisabledScope(_tree == null)) if (GUILayout.Button("Validate")) Debug.Log($"Behavior tree valid: {_tree.ValidateTree().IsValid}", _tree);
            EditorGUILayout.EndHorizontal();
            if (_tree == null) { EditorGUILayout.HelpBox("Create or select a BehaviorTreeDefinition to author enemy, NPC, boss, or companion AI.", MessageType.Info); return; }
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            UnityEditor.Editor.CreateCachedEditor(_tree, null, ref _editor);
            _editor.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Preview", EditorStyles.boldLabel);
            foreach (var node in _tree.Nodes) if (node != null) EditorGUILayout.LabelField(node.nodeId, $"{node.kind} → {string.Join(", ", node.children ?? new System.Collections.Generic.List<string>())}");
            EditorGUILayout.EndScrollView();
        }
    }
}
