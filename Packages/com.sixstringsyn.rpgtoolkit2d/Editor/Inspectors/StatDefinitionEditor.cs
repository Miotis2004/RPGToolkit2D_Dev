using SixStringSyn.RPGToolkit2D.Runtime.Stats;
using UnityEditor;

namespace SixStringSyn.RPGToolkit2D.Editor.Inspectors
{
    [CustomEditor(typeof(StatDefinition))]
    public sealed class StatDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
