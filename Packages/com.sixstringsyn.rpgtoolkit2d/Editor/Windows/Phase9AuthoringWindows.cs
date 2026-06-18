using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Editor.Dashboard;
using SixStringSyn.RPGToolkit2D.Runtime.Abilities;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Loot;
using SixStringSyn.RPGToolkit2D.Runtime.Vendors;
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
            EditorGUILayout.HelpBox($"Create, select, and inspect {typeName.ToLowerInvariant()} assets through the Project window. This lightweight helper provides a stable menu entry for RPG Toolkit authoring workflows.", MessageType.Info);
            if (GUILayout.Button($"Create {typeName} Asset")) CreateAsset();
        }
        protected abstract void CreateAsset();
    }

    public sealed class CombatTuningEditorWindow : AssetPickerWindow { [MenuItem("Tools/RPG Toolkit/Combat Tuning Editor")] public static void Open() { var w = GetWindow<CombatTuningEditorWindow>("Combat Tuning"); w.TypeName = "Combat Tuning"; } protected override void CreateAsset() { Selection.activeObject = null; } }

    public abstract class RPGAssetEditorWindow<TAsset> : EditorWindow where TAsset : RPGObject
    {
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private string _search = string.Empty;
        protected TAsset Selected;
        protected SerializedObject SerializedAsset;

        protected abstract string Heading { get; }
        protected abstract string HelpText { get; }
        protected abstract string SectionTitle { get; }
        protected abstract string DuplicateUndoName { get; }
        protected virtual string SelectHelp => $"Select or create a {Heading.ToLowerInvariant()} asset to edit domain-specific fields and review validation issues.";

        protected void DrawEditorGUI()
        {
            EditorGUILayout.LabelField(Heading, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(HelpText, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            DrawList();
            DrawDetails();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(330f));
            _search = EditorGUILayout.TextField("Search", _search);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create")) Select(CreateAsset());
            using (new EditorGUI.DisabledScope(Selected == null))
            {
                if (GUILayout.Button("Duplicate")) Select(DuplicateAsset(Selected));
                if (GUILayout.Button("Ping")) EditorGUIUtility.PingObject(Selected);
                if (GUILayout.Button("Select")) Selection.activeObject = Selected;
            }
            EditorGUILayout.EndHorizontal();

            var assets = FindAssets(_search);
            EditorGUILayout.LabelField($"{assets.Count} asset(s) found", EditorStyles.miniLabel);
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, EditorStyles.helpBox);
            foreach (var asset in assets)
            {
                var label = BuildListLabel(asset);
                if (GUILayout.Toggle(Selected == asset, label, EditorStyles.miniButton)) Select(asset);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetails()
        {
            EditorGUILayout.BeginVertical();
            if (Selected == null)
            {
                EditorGUILayout.HelpBox(SelectHelp, MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            if (SerializedAsset == null || SerializedAsset.targetObject != Selected) SerializedAsset = new SerializedObject(Selected);
            SerializedAsset.Update();
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Asset", Selected, typeof(TAsset), false);
            if (GUILayout.Button("Ping", GUILayout.Width(70f))) EditorGUIUtility.PingObject(Selected);
            if (GUILayout.Button("Select", GUILayout.Width(70f))) Selection.activeObject = Selected;
            EditorGUILayout.EndHorizontal();
            DrawSharedFields();
            DrawDomainFields();
            SerializedAsset.ApplyModifiedProperties();
            DrawValidationSummary();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawSharedFields()
        {
            EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_id"));
            EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_displayName"));
            EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_description"));
            EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_tags"), true);
        }

        protected abstract void DrawDomainFields();
        protected abstract RPGValidationResult ValidateSelected(TAsset asset, IEnumerable<TAsset> allAssets);
        protected virtual string BuildListLabel(TAsset asset) => $"{asset.DisplayName}  [{asset.Id}]";

        protected void DrawValidationSummary()
        {
            var result = ValidateSelected(Selected, FindAssets(null));
            EditorGUILayout.LabelField("Validation Summary", EditorStyles.boldLabel);
            if (result.Messages.Count == 0) EditorGUILayout.HelpBox($"{Heading} is valid.", MessageType.Info);
            foreach (var message in result.Messages) EditorGUILayout.HelpBox($"[{message.Code}] {message.Message}", message.IsError ? MessageType.Error : MessageType.Warning);
        }

        protected IReadOnlyList<TAsset> FindAssets(string search)
        {
            var section = RPGToolkitAuthoringWorkflow.Sections.First(s => s.Title == SectionTitle);
            return RPGToolkitAuthoringWorkflow.FindAssets(section).Select(entry => entry.Asset as TAsset).Where(asset => MatchesSearch(asset, search)).OrderBy(asset => asset.DisplayName).ToList();
        }

        private static bool MatchesSearch(TAsset asset, string search)
        {
            if (asset == null) return false;
            if (string.IsNullOrWhiteSpace(search)) return true;
            var path = AssetDatabase.GetAssetPath(asset) ?? string.Empty;
            return asset.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || asset.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || asset.Id.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private TAsset CreateAsset() => RPGToolkitAuthoringWorkflow.CreateAsset(RPGToolkitAuthoringWorkflow.Sections.First(s => s.Title == SectionTitle)) as TAsset;

        private TAsset DuplicateAsset(TAsset source)
        {
            if (source == null) return null;
            var clone = Instantiate(source);
            clone.AssignNewId();
            var sourcePath = AssetDatabase.GetAssetPath(source);
            var folder = string.IsNullOrWhiteSpace(sourcePath) ? RPGToolkitAuthoringWorkflow.DefaultAssetFolder : Path.GetDirectoryName(sourcePath).Replace('\\', '/');
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, source.name + " Copy.asset").Replace('\\', '/'));
            AssetDatabase.CreateAsset(clone, path);
            AssetDatabase.SaveAssets();
            Undo.RegisterCreatedObjectUndo(clone, DuplicateUndoName);
            return clone;
        }

        protected void Select(TAsset asset)
        {
            Selected = asset;
            SerializedAsset = asset == null ? null : new SerializedObject(asset);
            Selection.activeObject = asset;
        }

        protected static RPGValidationResult ValidateCommon(TAsset asset, IEnumerable<TAsset> allAssets, string prefix, string noun)
        {
            var result = new RPGValidationResult();
            if (asset == null)
            {
                result.AddError(prefix + "_NULL", noun + " definition is missing.");
                return result;
            }

            if (asset.Id.IsEmpty) result.AddError(prefix + "_MISSING_ID", noun + " is missing an RPG ID.");
            var serialized = new SerializedObject(asset);
            if (string.IsNullOrWhiteSpace(serialized.FindProperty("_displayName")?.stringValue)) result.AddError(prefix + "_MISSING_DISPLAY_NAME", noun + " needs an authored display name instead of relying on the asset filename.", asset.Id);
            if (allAssets != null && !asset.Id.IsEmpty && allAssets.Count(other => other != null && other != asset && other.Id == asset.Id) > 0) result.AddError(prefix + "_DUPLICATE_ID", noun + " ID is used by multiple assets.", asset.Id);
            return result;
        }
    }

    public sealed class AbilityEditorWindow : RPGAssetEditorWindow<AbilityDefinition>
    {
        [MenuItem("Tools/RPG Toolkit/Ability Editor")] public static void Open() { var w = GetWindow<AbilityEditorWindow>("Abilities"); w.minSize = new Vector2(860f, 520f); w.Show(); }
        protected override string Heading => "Ability Editor";
        protected override string HelpText => "Browse, create, duplicate, validate, and edit AbilityDefinition assets including targeting, cooldowns, resource costs, tags, and effects.";
        protected override string SectionTitle => "Abilities";
        protected override string DuplicateUndoName => "Duplicate Ability";
        private void OnGUI() => DrawEditorGUI();
        protected override void DrawDomainFields() { EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_targetingMode")); EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_range")); EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_cooldownSeconds")); EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_costs"), true); EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_effects"), true); }
        protected override RPGValidationResult ValidateSelected(AbilityDefinition asset, IEnumerable<AbilityDefinition> allAssets) => ValidateAbility(asset, allAssets);
        public static RPGValidationResult ValidateAbility(AbilityDefinition ability, IEnumerable<AbilityDefinition> allAbilities = null)
        {
            var result = ValidateCommon(ability, allAbilities, "RPG_ABILITY", "Ability");
            if (ability == null) return result;
            var serialized = new SerializedObject(ability);
            if ((serialized.FindProperty("_cooldownSeconds")?.floatValue ?? 0f) < 0f) result.AddError("RPG_ABILITY_INVALID_COOLDOWN", "Ability cooldown cannot be negative.", ability.Id);
            if ((serialized.FindProperty("_range")?.floatValue ?? 0f) < 0f) result.AddError("RPG_ABILITY_INVALID_RANGE", "Ability range cannot be negative.", ability.Id);
            foreach (var cost in ability.Costs) { if (cost == null || cost.resource == null) result.AddError("RPG_ABILITY_MISSING_COST_RESOURCE", "Ability cost needs a resource reference.", ability.Id); if (cost != null && cost.amount < 0f) result.AddError("RPG_ABILITY_INVALID_COST", "Ability cost amount cannot be negative.", ability.Id); }
            if (ability.Effects.Count == 0) result.AddWarning("RPG_ABILITY_NO_EFFECTS", "Ability has no runtime effects.", ability.Id);
            foreach (var effect in ability.Effects) if (effect == null || effect.targetResource == null) result.AddWarning("RPG_ABILITY_EFFECT_NO_TARGET_RESOURCE", "Ability effect has no target resource reference.", ability.Id);
            return result;
        }
    }

    public sealed class VendorEditorWindow : RPGAssetEditorWindow<VendorDefinition>
    {
        [MenuItem("Tools/RPG Toolkit/Vendor Editor")] public static void Open() { var w = GetWindow<VendorEditorWindow>("Vendors"); w.minSize = new Vector2(860f, 520f); w.Show(); }
        protected override string Heading => "Vendor Editor";
        protected override string HelpText => "Browse, create, duplicate, validate, and edit VendorDefinition assets with stock tables, prices, quantities, and sell multipliers.";
        protected override string SectionTitle => "Vendors";
        protected override string DuplicateUndoName => "Duplicate Vendor";
        private void OnGUI() => DrawEditorGUI();
        protected override void DrawDomainFields() { EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_sellMultiplier")); EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_stock"), true); if (GUILayout.Button("Open Item Database")) ItemDatabaseWindow.Open(); }
        protected override RPGValidationResult ValidateSelected(VendorDefinition asset, IEnumerable<VendorDefinition> allAssets) => ValidateVendor(asset, allAssets);
        public static RPGValidationResult ValidateVendor(VendorDefinition vendor, IEnumerable<VendorDefinition> allVendors = null)
        {
            var result = ValidateCommon(vendor, allVendors, "RPG_VENDOR", "Vendor");
            if (vendor == null) return result;
            if (vendor.Stock.Count == 0) result.AddWarning("RPG_VENDOR_EMPTY_STOCK", "Vendor has no stock entries.", vendor.Id);
            var seen = new HashSet<ItemDefinition>();
            foreach (var entry in vendor.Stock)
            {
                if (entry == null || entry.item == null) result.AddError("RPG_VENDOR_MISSING_ITEM", "Vendor stock entry needs an item reference.", vendor.Id);
                else if (!seen.Add(entry.item)) result.AddError("RPG_VENDOR_DUPLICATE_STOCK_ITEM", "Vendor stock contains duplicate item entries.", vendor.Id);
                if (entry != null && entry.price <= 0) result.AddError("RPG_VENDOR_INVALID_PRICE", "Vendor stock price must be greater than zero.", vendor.Id);
                if (entry != null && entry.quantity < 0) result.AddError("RPG_VENDOR_INVALID_QUANTITY", "Vendor stock quantity cannot be negative.", vendor.Id);
            }
            return result;
        }
    }

    public sealed class LootTableEditorWindow : RPGAssetEditorWindow<LootTableDefinition>
    {
        private int _simulationRolls = 100;
        private int _simulationSeed = 1234;
        [MenuItem("Tools/RPG Toolkit/Loot Table Editor")] public static void Open() { var w = GetWindow<LootTableEditorWindow>("Loot Tables"); w.minSize = new Vector2(860f, 540f); w.Show(); }
        protected override string Heading => "Loot Table Editor";
        protected override string HelpText => "Browse, create, duplicate, validate, edit weighted loot entries, and preview deterministic drop simulations.";
        protected override string SectionTitle => "Loot Tables";
        protected override string DuplicateUndoName => "Duplicate Loot Table";
        private void OnGUI() => DrawEditorGUI();
        protected override void DrawDomainFields()
        {
            EditorGUILayout.PropertyField(SerializedAsset.FindProperty("_entries"), true);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Simulation Preview", EditorStyles.boldLabel);
            _simulationRolls = EditorGUILayout.IntField("Rolls", Math.Max(1, _simulationRolls));
            _simulationSeed = EditorGUILayout.IntField("Seed", _simulationSeed);
            if (GUILayout.Button("Run Preview Rolls")) ShowSimulationPreview();
        }
        protected override RPGValidationResult ValidateSelected(LootTableDefinition asset, IEnumerable<LootTableDefinition> allAssets) => ValidateLootTable(asset, allAssets);
        public static RPGValidationResult ValidateLootTable(LootTableDefinition table, IEnumerable<LootTableDefinition> allTables = null)
        {
            var result = ValidateCommon(table, allTables, "RPG_LOOT_TABLE", "Loot table");
            if (table == null) return result;
            var tableResult = table.ValidateTable();
            foreach (var message in tableResult.Messages) { if (message.IsError) result.AddError("RPG_LOOT_TABLE_" + message.Code.ToUpperInvariant().Replace('.', '_'), message.Message, table.Id); else result.AddWarning("RPG_LOOT_TABLE_" + message.Code.ToUpperInvariant().Replace('.', '_'), message.Message, table.Id); }
            foreach (var entry in table.Entries) if (entry != null && entry.item != null && entry.nestedTable != null) result.AddWarning("RPG_LOOT_TABLE_AMBIGUOUS_ENTRY", "Loot entry has both an item and nested table; nested table takes precedence at roll time.", table.Id);
            return result;
        }
        private void ShowSimulationPreview()
        {
            var simulation = LootRoller.Simulate(Selected, _simulationRolls, _simulationSeed);
            var groups = simulation.drops.GroupBy(drop => drop.Definition.DisplayName).OrderByDescending(group => group.Count()).Select(group => $"{group.Key}: {group.Count()} ({(group.Count() * 100f / Math.Max(1, simulation.rolls)):0.0}%)");
            EditorUtility.DisplayDialog("Loot Simulation Preview", simulation.drops.Count == 0 ? "No drops produced." : string.Join("\n", groups), "OK");
        }
    }
}
