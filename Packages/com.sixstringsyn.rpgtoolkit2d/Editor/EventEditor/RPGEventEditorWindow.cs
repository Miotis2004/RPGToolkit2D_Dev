using SixStringSyn.RPGToolkit2D.Runtime.Events;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.EventEditor
{
    public sealed class RPGEventEditorWindow : EditorWindow
    {
        private RPGEventDefinition _eventDefinition;
        private UnityEditor.Editor _assetEditor;

        [MenuItem("Window/RPG Toolkit/Event Editor")]
        public static void Open() => GetWindow<RPGEventEditorWindow>("RPG Event Editor");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("RPG Toolkit Event Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Author RPG Maker-style event command lists, command conditions, and integration payloads for dialogue, quests, inventory, audio, shops, variables, and teleporting.", MessageType.Info);
            var next = (RPGEventDefinition)EditorGUILayout.ObjectField("Event", _eventDefinition, typeof(RPGEventDefinition), false);
            if (next != _eventDefinition)
            {
                _eventDefinition = next;
                if (_assetEditor != null) DestroyImmediate(_assetEditor);
                _assetEditor = _eventDefinition != null ? UnityEditor.Editor.CreateEditor(_eventDefinition) : null;
            }
            if (_assetEditor != null) _assetEditor.OnInspectorGUI();
            else if (GUILayout.Button("Create Event Asset")) CreateEventAsset();
        }

        private void CreateEventAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create RPG Event", "RPGEvent", "asset", "Choose where to create the event asset.");
            if (string.IsNullOrWhiteSpace(path)) return;
            var asset = CreateInstance<RPGEventDefinition>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _eventDefinition = asset;
            _assetEditor = UnityEditor.Editor.CreateEditor(_eventDefinition);
        }
    }
}
