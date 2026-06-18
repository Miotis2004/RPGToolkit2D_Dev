using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.DialogueGraph
{
    public sealed class DialogueGraphEditorWindow : EditorWindow
    {
        [SerializeField] private DialogueDefinition _dialogue;
        [SerializeField] private string _selectedNodeId;
        private Vector2 _canvasScroll;
        private Vector2 _detailScroll;
        private string _search = string.Empty;
        private RPGValidationResult _lastValidation;

        [MenuItem("Tools/RPG Toolkit/Dialogue Graph Editor")]
        public static void Open() => GetWindow<DialogueGraphEditorWindow>("Dialogue Graph");

        public static IReadOnlyList<DialogueNode> SearchNodes(DialogueDefinition dialogue, string search)
        {
            if (dialogue == null) return Array.Empty<DialogueNode>();
            return dialogue.Nodes.Where(node => MatchesSearch(node, search)).ToList();
        }

        private void OnGUI()
        {
            DrawToolbar();
            _search = EditorGUILayout.TextField(new GUIContent("Search", "Search speaker, localization key, node text, choice text, condition keys, and command payloads."), _search);
            if (_dialogue == null) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawNodeOutline();
                DrawStructuredGraph();
                DrawNodeDetails();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _dialogue = (DialogueDefinition)EditorGUILayout.ObjectField(_dialogue, typeof(DialogueDefinition), false, GUILayout.MinWidth(220));
                if (GUILayout.Button("Create Dialogue Asset", EditorStyles.toolbarButton)) CreateAsset();
                using (new EditorGUI.DisabledScope(_dialogue == null))
                {
                    if (GUILayout.Button("Add Line", EditorStyles.toolbarButton)) AddNode(DialogueNodeType.Line);
                    if (GUILayout.Button("Add Choice", EditorStyles.toolbarButton)) AddNode(DialogueNodeType.Choice);
                    if (GUILayout.Button("Add Quest", EditorStyles.toolbarButton)) AddNode(DialogueNodeType.QuestUpdate);
                    if (GUILayout.Button("Add Reward", EditorStyles.toolbarButton)) AddNode(DialogueNodeType.Reward);
                    if (GUILayout.Button("Add Exit", EditorStyles.toolbarButton)) AddNode(DialogueNodeType.Exit);
                    if (GUILayout.Button("Validate", EditorStyles.toolbarButton)) Validate();
                }
            }
        }

        private void DrawNodeOutline()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(230)))
            {
                EditorGUILayout.LabelField("Node Outline", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Nodes: {_dialogue.Nodes.Count}", EditorStyles.miniLabel);
                foreach (var node in SearchNodes(_dialogue, _search))
                {
                    var selected = node.NodeId == _selectedNodeId;
                    if (GUILayout.Toggle(selected, $"{node.NodeType}: {NodeTitle(node)}", EditorStyles.miniButton) && !selected) _selectedNodeId = node.NodeId;
                }
            }
        }

        private void DrawStructuredGraph()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(360)))
            {
                EditorGUILayout.LabelField("Structured Node-Link View", EditorStyles.boldLabel);
                _canvasScroll = EditorGUILayout.BeginScrollView(_canvasScroll, EditorStyles.helpBox);
                foreach (var node in SearchNodes(_dialogue, _search)) DrawNodeCard(node);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawNodeCard(DialogueNode node)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(NodeTitle(node), EditorStyles.boldLabel)) _selectedNodeId = node.NodeId;
                    if (_dialogue.EntryNodeId == node.NodeId) GUILayout.Label("ENTRY", EditorStyles.miniButton, GUILayout.Width(55));
                }

                EditorGUILayout.LabelField(node.Text, EditorStyles.wordWrappedMiniLabel);
                if (!string.IsNullOrWhiteSpace(node.NextNodeId)) DrawEdge($"Next → {DescribeNode(node.NextNodeId)}", NodeExists(node.NextNodeId));
                foreach (var choice in node.Choices) DrawEdge($"Choice: {choice.Text} → {DescribeNode(choice.TargetNodeId)}", NodeExists(choice.TargetNodeId));
            }
        }

        private void DrawNodeDetails()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Max(330, position.width * 0.32f))))
            {
                EditorGUILayout.LabelField("Node Detail", EditorStyles.boldLabel);
                var node = _dialogue.GetNode(_selectedNodeId) ?? _dialogue.EntryNode;
                if (node == null) { EditorGUILayout.HelpBox("Select or create a node to edit details.", MessageType.Info); return; }
                _selectedNodeId = node.NodeId;
                var nodeIndex = _dialogue.Nodes.ToList().IndexOf(node);
                var serialized = new SerializedObject(_dialogue);
                var nodeProperty = serialized.FindProperty("_nodes").GetArrayElementAtIndex(nodeIndex);

                _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
                EditorGUILayout.LabelField("Node ID", node.NodeId, EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("_nodeType"));
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("_speaker"));
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("_text"));
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("_localizationKey"));
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("_portrait"));
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("_speakerAnimation"));
                DrawTargetPopup(nodeProperty.FindPropertyRelative("_nextNodeId"), "Next Node");
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("_conditions"), true);
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("_commands"), true);
                DrawChoices(nodeProperty.FindPropertyRelative("_choices"));
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Set As Entry")) serialized.FindProperty("_entryNodeId").stringValue = node.NodeId;
                    if (GUILayout.Button("Ping Related Assets")) PingRelatedAssets(node);
                }
                serialized.ApplyModifiedProperties();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawChoices(SerializedProperty choices)
        {
            EditorGUILayout.LabelField("Explicit Choice Edges", EditorStyles.boldLabel);
            for (var i = 0; i < choices.arraySize; i++)
            {
                var choice = choices.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(choice.FindPropertyRelative("_text"));
                    DrawTargetPopup(choice.FindPropertyRelative("_targetNodeId"), "Target Node");
                    EditorGUILayout.PropertyField(choice.FindPropertyRelative("_conditions"), true);
                    EditorGUILayout.PropertyField(choice.FindPropertyRelative("_commands"), true);
                    if (GUILayout.Button("Remove Choice")) { choices.DeleteArrayElementAtIndex(i); break; }
                }
            }
            if (GUILayout.Button("Add Choice Edge")) choices.InsertArrayElementAtIndex(choices.arraySize);
        }

        private void DrawTargetPopup(SerializedProperty target, string label)
        {
            var options = new[] { "<none>" }.Concat(_dialogue.Nodes.Select(NodeTitle)).ToArray();
            var values = new[] { string.Empty }.Concat(_dialogue.Nodes.Select(node => node.NodeId)).ToArray();
            var current = Mathf.Max(0, Array.IndexOf(values, target.stringValue));
            target.stringValue = values[EditorGUILayout.Popup(label, current, options)];
        }

        private static bool MatchesSearch(DialogueNode node, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return true;
            var haystack = string.Join("\n", new[] { node.Speaker, node.Text, node.LocalizationKey, node.SpeakerAnimation, node.NodeId }
                .Concat(node.Choices.Select(choice => choice.Text))
                .Concat(node.Conditions.Select(condition => condition.Key + condition.Value))
                .Concat(node.Commands.Select(command => command.Name + command.Argument))
                .Concat(node.Choices.SelectMany(choice => choice.Conditions.Select(condition => condition.Key + condition.Value)))
                .Concat(node.Choices.SelectMany(choice => choice.Commands.Select(command => command.Name + command.Argument))));
            return haystack.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void DrawEdge(string label, bool valid) => EditorGUILayout.HelpBox(label, valid ? MessageType.Info : MessageType.Error);
        private bool NodeExists(string id) => string.IsNullOrWhiteSpace(id) || _dialogue.GetNode(id) != null;
        private string DescribeNode(string id) => _dialogue.GetNode(id) is DialogueNode node ? NodeTitle(node) : $"Missing ({id})";
        private static string NodeTitle(DialogueNode node) => string.IsNullOrWhiteSpace(node.Text) ? node.NodeId : node.Text.Length > 42 ? node.Text.Substring(0, 42) + "…" : node.Text;

        private void CreateAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Dialogue", "NewDialogueDefinition", "asset", "Choose a dialogue asset path.");
            if (string.IsNullOrWhiteSpace(path)) return;
            _dialogue = CreateInstance<DialogueDefinition>();
            _selectedNodeId = _dialogue.AddNode(DialogueNodeType.Entry, "NPC", "Hello.").NodeId;
            AssetDatabase.CreateAsset(_dialogue, path);
            AssetDatabase.SaveAssets();
        }

        private void AddNode(DialogueNodeType type)
        {
            Undo.RecordObject(_dialogue, "Add Dialogue Node");
            var text = type == DialogueNodeType.Exit ? "End" : type == DialogueNodeType.QuestUpdate ? "Quest update" : type == DialogueNodeType.Reward ? "Reward" : "New line";
            _selectedNodeId = _dialogue.AddNode(type, type == DialogueNodeType.Exit ? string.Empty : "Speaker", text).NodeId;
            EditorUtility.SetDirty(_dialogue);
        }

        private void Validate()
        {
            _lastValidation = _dialogue.ValidateGraph();
            if (_lastValidation.IsValid) Debug.Log($"Dialogue '{_dialogue.name}' is valid.", _dialogue);
            else foreach (var message in _lastValidation.Messages) Debug.LogError(message.Message, _dialogue);
        }

        private void PingRelatedAssets(DialogueNode node)
        {
            var terms = new[] { node.NodeId, node.LocalizationKey }.Where(term => !string.IsNullOrWhiteSpace(term)).ToArray();
            var guids = AssetDatabase.FindAssets("t:NPCDefinition t:QuestDefinition t:ItemDefinition");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (terms.Any(term => path.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)) { EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)); return; }
            }
            Debug.Log($"No NPC, Quest, or Item assets with filenames matching node '{NodeTitle(node)}' were found.", _dialogue);
        }
    }
}
