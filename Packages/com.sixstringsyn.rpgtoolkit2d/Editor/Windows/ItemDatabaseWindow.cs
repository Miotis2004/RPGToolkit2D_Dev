using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class ItemDatabaseWindow : EditorWindow
    {
        [MenuItem("Tools/RPG Toolkit/Item Database")]
        public static void Open() => GetWindow<ItemDatabaseWindow>("RPG Items");

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh Items")) Repaint();
            var guids = AssetDatabase.FindAssets("t:ItemDefinition");
            EditorGUILayout.LabelField($"Items: {guids.Length}", EditorStyles.boldLabel);
            foreach (var item in guids.Select(guid => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid))).Where(item => item != null))
            {
                EditorGUILayout.LabelField(item.DisplayName, $"{item.ItemType} | {item.Rarity} | Stack {item.MaximumStackSize}");
            }
        }
    }
}
