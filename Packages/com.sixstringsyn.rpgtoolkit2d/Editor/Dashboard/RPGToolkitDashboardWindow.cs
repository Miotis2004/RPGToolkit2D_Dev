using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Dashboard
{
    public sealed class RPGToolkitDashboardWindow : EditorWindow
    {
        [MenuItem("Tools/RPG Toolkit/Dashboard")]
        public static void Open()
        {
            var window = GetWindow<RPGToolkitDashboardWindow>("RPG Toolkit");
            window.minSize = new Vector2(420f, 260f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("RPG Toolkit 2D", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Package foundation is installed and ready for future RPG systems.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            foreach (var result in RPGToolkitPackageValidator.ValidatePackageFoundation())
            {
                var icon = result.Passed ? "✓" : "✗";
                EditorGUILayout.LabelField($"{icon} {result.RuleName}", result.Message, EditorStyles.wordWrappedLabel);
            }
        }
    }
}
