using SixStringSyn.RPGToolkit2D.Runtime.Crafting;
using SixStringSyn.RPGToolkit2D.Runtime.Factions;
using SixStringSyn.RPGToolkit2D.Runtime.Loot;
using SixStringSyn.RPGToolkit2D.Runtime.SkillTrees;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.AdvancedGameplay
{
    public abstract class AdvancedGameplayAssetWindow<T> : EditorWindow where T : ScriptableObject
    {
        private T _asset; private UnityEditor.Editor _editor;
        protected abstract string DefaultAssetName { get; }
        protected virtual void OnGUI() { EditorGUILayout.LabelField(titleContent.text, EditorStyles.boldLabel); _asset = (T)EditorGUILayout.ObjectField("Asset", _asset, typeof(T), false); EditorGUILayout.BeginHorizontal(); if (GUILayout.Button("Create")) ProjectWindowUtil.CreateAsset(CreateInstance<T>(), DefaultAssetName); if (_asset != null && GUILayout.Button("Validate")) ValidateAsset(_asset); EditorGUILayout.EndHorizontal(); if (_asset == null) return; UnityEditor.Editor.CreateCachedEditor(_asset, null, ref _editor); _editor.OnInspectorGUI(); }
        protected virtual void ValidateAsset(T asset) => Debug.Log($"{asset.name} selected for advanced gameplay authoring.", asset);
    }
    public sealed class FactionDatabaseEditorWindow : AdvancedGameplayAssetWindow<FactionDatabase> { [MenuItem("Tools/RPG Toolkit/Faction Database Editor")] public static void Open() => GetWindow<FactionDatabaseEditorWindow>("Factions"); protected override string DefaultAssetName => "FactionDatabase.asset"; protected override void ValidateAsset(FactionDatabase a) => Debug.Log($"Faction database valid: {a.ValidateDatabase().IsValid}", a); }
    public sealed class SkillTreeEditorWindow : AdvancedGameplayAssetWindow<SkillTreeDefinition> { [MenuItem("Tools/RPG Toolkit/Skill Tree Designer")] public static void Open() => GetWindow<SkillTreeEditorWindow>("Skill Trees"); protected override string DefaultAssetName => "NewSkillTree.asset"; protected override void ValidateAsset(SkillTreeDefinition a) => Debug.Log($"Skill tree valid: {a.ValidateTree().IsValid}", a); }
    public sealed class CraftingRecipeDatabaseWindow : AdvancedGameplayAssetWindow<CraftingDatabase> { [MenuItem("Tools/RPG Toolkit/Crafting Editor")] public static void Open() => GetWindow<CraftingRecipeDatabaseWindow>("Crafting"); protected override string DefaultAssetName => "CraftingRecipeDatabase.asset"; protected override void ValidateAsset(CraftingDatabase a) => Debug.Log($"Crafting database valid: {a.ValidateDatabase().IsValid}", a); }
    public sealed class AdvancedLootTableEditorWindow : AdvancedGameplayAssetWindow<LootTableDefinition> { [MenuItem("Tools/RPG Toolkit/Advanced Loot Table Editor")] public static void Open() => GetWindow<AdvancedLootTableEditorWindow>("Advanced Loot"); protected override string DefaultAssetName => "NewLootTable.asset"; protected override void ValidateAsset(LootTableDefinition a) { var result = LootRoller.Simulate(a, 100, 1234); Debug.Log($"Loot table valid: {a.ValidateTable().IsValid}; simulated {result.drops.Count} drops / {result.rolls} rolls.", a); } }
}
