using SixStringSyn.RPGToolkit2D.Runtime.Saving;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class SaveDataDebuggerWindow : EditorWindow
    {
        private SaveSlotService _slots;
        private Vector2 _scroll;

        [MenuItem("Tools/RPG Toolkit/Save Data Debugger")]
        public static void Open() => GetWindow<SaveDataDebuggerWindow>("Save Data Debugger");

        private void OnEnable() => _slots = new SaveSlotService(new SaveGameService());

        private void OnGUI()
        {
            EditorGUILayout.LabelField("RPG Toolkit Save Slots", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(_slots.RootPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (GUILayout.Button("Refresh")) Repaint();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var slot in _slots.EnumerateSlots())
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(slot.SlotId, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Scene", slot.Metadata?.sceneName ?? "Unknown");
                EditorGUILayout.LabelField("Version", slot.Metadata?.saveVersion ?? "Unreadable");
                EditorGUILayout.LabelField("Updated UTC", slot.Metadata?.updatedUtc ?? "Unknown");
                EditorGUILayout.SelectableLabel(slot.Path, EditorStyles.miniLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
