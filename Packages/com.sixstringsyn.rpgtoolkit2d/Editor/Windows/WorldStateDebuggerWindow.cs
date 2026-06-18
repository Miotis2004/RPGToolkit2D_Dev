#if UNITY_EDITOR
using SixStringSyn.RPGToolkit2D.Runtime.World;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class WorldStateDebuggerWindow : EditorWindow
    {
        private readonly WorldState _state = new WorldState(); private string _key; private string _value;
        [MenuItem("Tools/RPG Toolkit/World State Debugger")]
        public static void Open() => GetWindow<WorldStateDebuggerWindow>("World State");
        private void OnGUI()
        {
            EditorGUILayout.LabelField("World State Debugger", EditorStyles.boldLabel);
            _key = EditorGUILayout.TextField("Key", _key); _value = EditorGUILayout.TextField("Value", _value);
            using (new EditorGUILayout.HorizontalScope()) { if (GUILayout.Button("Set")) _state.SetString(_key, _value); if (GUILayout.Button("Clear")) _state.Clear(_key); }
            foreach (var pair in _state.Values) EditorGUILayout.LabelField(pair.Key, pair.Value);
        }
    }
}
#endif
