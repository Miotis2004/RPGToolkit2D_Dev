using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public abstract class AssetPickerWindow : EditorWindow
    {
        protected string TypeName;
        protected void OnGUI()
        {
            var typeName = string.IsNullOrEmpty(TypeName) ? titleContent.text : TypeName;
            EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"Create, select, and inspect {typeName.ToLowerInvariant()} assets through the Project window. This lightweight Phase 9 window provides a stable menu entry for RPG Toolkit authoring workflows.", MessageType.Info);
            if (GUILayout.Button($"Create {typeName} Asset")) CreateAsset();
        }
        protected abstract void CreateAsset();
    }

    public sealed class CombatTuningEditorWindow : AssetPickerWindow { [MenuItem("Tools/RPG Toolkit/Combat Tuning Editor")] public static void Open() { var w = GetWindow<CombatTuningEditorWindow>("Combat Tuning"); w.TypeName = "Combat Tuning"; } protected override void CreateAsset() { Selection.activeObject = null; } }
    public sealed class AbilityEditorWindow : AssetPickerWindow { [MenuItem("Tools/RPG Toolkit/Ability Editor")] public static void Open() { var w = GetWindow<AbilityEditorWindow>("Abilities"); w.TypeName = "Ability"; } protected override void CreateAsset() { ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<global::SixStringSyn.RPGToolkit2D.Runtime.Abilities.AbilityDefinition>(), "NewAbilityDefinition.asset"); } }
    public sealed class LootTableEditorWindow : AssetPickerWindow { [MenuItem("Tools/RPG Toolkit/Loot Table Editor")] public static void Open() { var w = GetWindow<LootTableEditorWindow>("Loot Tables"); w.TypeName = "Loot Table"; } protected override void CreateAsset() { ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<global::SixStringSyn.RPGToolkit2D.Runtime.Loot.LootTableDefinition>(), "NewLootTable.asset"); } }
    public sealed class VendorEditorWindow : AssetPickerWindow { [MenuItem("Tools/RPG Toolkit/Vendor Editor")] public static void Open() { var w = GetWindow<VendorEditorWindow>("Vendors"); w.TypeName = "Vendor"; } protected override void CreateAsset() { ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<global::SixStringSyn.RPGToolkit2D.Runtime.Vendors.VendorDefinition>(), "NewVendor.asset"); } }
}
