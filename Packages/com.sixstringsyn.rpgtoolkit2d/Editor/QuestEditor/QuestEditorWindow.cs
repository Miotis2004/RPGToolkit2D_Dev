using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.QuestEditor
{
    public sealed class QuestEditorWindow : EditorWindow
    {
        [SerializeField] private QuestDefinition _quest;
        private UnityEditor.Editor _assetEditor;

        [MenuItem("Tools/RPG Toolkit/Quest Editor")]
        public static void Open() => GetWindow<QuestEditorWindow>("Quest Editor");

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _quest = (QuestDefinition)EditorGUILayout.ObjectField("Quest", _quest, typeof(QuestDefinition), false);
            if (EditorGUI.EndChangeCheck()) ResetEditor();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Quest Asset")) CreateAsset();
                using (new EditorGUI.DisabledScope(_quest == null))
                {
                    if (GUILayout.Button("Validate")) Validate();
                    if (GUILayout.Button("Select")) Selection.activeObject = _quest;
                }
            }
            if (_quest == null) return;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(_quest.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Objectives", _quest.Objectives.Count.ToString());
            EditorGUILayout.LabelField("Rewards", _quest.Rewards.Count.ToString());
            _assetEditor ??= UnityEditor.Editor.CreateEditor(_quest);
            _assetEditor.OnInspectorGUI();
        }

        private void CreateAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Quest", "NewQuestDefinition", "asset", "Choose a quest asset path.");
            if (string.IsNullOrWhiteSpace(path)) return;
            _quest = CreateInstance<QuestDefinition>();
            AssetDatabase.CreateAsset(_quest, path);
            AssetDatabase.SaveAssets();
            ResetEditor();
        }
        private void Validate()
        {
            var result = _quest.ValidateQuest();
            if (result.IsValid) Debug.Log($"Quest '{_quest.name}' is valid.", _quest);
            else foreach (var message in result.Messages) if (message.IsError) Debug.LogError(message.Message, _quest); else Debug.LogWarning(message.Message, _quest);
        }
        private void OnDisable() => ResetEditor();
        private void ResetEditor() { if (_assetEditor != null) DestroyImmediate(_assetEditor); _assetEditor = null; }
    }
}
