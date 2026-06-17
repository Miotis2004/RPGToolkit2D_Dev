using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.DialogueGraph
{
    public sealed class DialogueGraphEditorWindow : EditorWindow
    {
        [SerializeField] private DialogueDefinition _dialogue;
        private string _search = string.Empty;

        [MenuItem("Tools/RPG Toolkit/Dialogue Graph Editor")]
        public static void Open() => GetWindow<DialogueGraphEditorWindow>("Dialogue Graph");

        private void OnGUI()
        {
            _dialogue = (DialogueDefinition)EditorGUILayout.ObjectField("Dialogue", _dialogue, typeof(DialogueDefinition), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Dialogue Asset")) CreateAsset();
                using (new EditorGUI.DisabledScope(_dialogue == null))
                {
                    if (GUILayout.Button("Add Line Node")) AddNode(DialogueNodeType.Line);
                    if (GUILayout.Button("Add Exit Node")) AddNode(DialogueNodeType.Exit);
                    if (GUILayout.Button("Validate")) Validate();
                }
            }

            _search = EditorGUILayout.TextField("Search", _search);
            if (_dialogue == null) return;
            EditorGUILayout.LabelField($"Nodes: {_dialogue.Nodes.Count}", EditorStyles.boldLabel);
            foreach (var node in _dialogue.Nodes.Where(MatchesSearch))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(node.NodeType.ToString(), node.Text);
                EditorGUILayout.LabelField("Speaker", node.Speaker);
                EditorGUILayout.LabelField("Choices", node.Choices.Count.ToString());
                EditorGUILayout.EndVertical();
            }
        }

        private bool MatchesSearch(DialogueNode node)
        {
            return string.IsNullOrWhiteSpace(_search) || node.Text.ToLowerInvariant().Contains(_search.ToLowerInvariant()) || node.Speaker.ToLowerInvariant().Contains(_search.ToLowerInvariant());
        }

        private void CreateAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Dialogue", "NewDialogueDefinition", "asset", "Choose a dialogue asset path.");
            if (string.IsNullOrWhiteSpace(path)) return;
            _dialogue = CreateInstance<DialogueDefinition>();
            _dialogue.AddNode(DialogueNodeType.Entry, "NPC", "Hello.");
            AssetDatabase.CreateAsset(_dialogue, path);
            AssetDatabase.SaveAssets();
        }

        private void AddNode(DialogueNodeType type)
        {
            Undo.RecordObject(_dialogue, "Add Dialogue Node");
            _dialogue.AddNode(type, type == DialogueNodeType.Exit ? string.Empty : "Speaker", type == DialogueNodeType.Exit ? "End" : "New line");
            EditorUtility.SetDirty(_dialogue);
        }

        private void Validate()
        {
            var result = _dialogue.ValidateGraph();
            if (result.IsValid) Debug.Log($"Dialogue '{_dialogue.name}' is valid.", _dialogue);
            else foreach (var message in result.Messages) Debug.LogError(message.Message, _dialogue);
        }
    }
}
