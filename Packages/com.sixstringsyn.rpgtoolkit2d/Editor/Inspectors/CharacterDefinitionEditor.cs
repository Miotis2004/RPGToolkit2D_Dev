using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using UnityEditor;

namespace SixStringSyn.RPGToolkit2D.Editor.Inspectors
{
    [CustomEditor(typeof(CharacterDefinition))]
    public sealed class CharacterDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
