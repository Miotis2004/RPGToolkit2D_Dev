using SixStringSyn.RPGToolkit2D.Editor;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Dashboard
{
    public sealed class RPGToolkitDashboardWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _searchText = string.Empty;
        private int _selectedSection;
        private RPGMapProjectValidationReport _mapValidationReport;
        private readonly Dictionary<string, bool> _expandedCards = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> _lastValidationResults = new Dictionary<string, string>();

        [MenuItem("Tools/RPG Toolkit/Dashboard")]
        public static void Open()
        {
            var window = GetWindow<RPGToolkitDashboardWindow>("RPG Toolkit");
            window.minSize = new Vector2(680f, 520f);
            window.Show();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawHeader();
            DrawQuickStart();
            DrawCapabilityLegend();
            DrawAuthoringSections();
            DrawDatabaseBrowser();
            DrawUtilities();
            DrawValidation();
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("RPG Toolkit 2D", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Central dashboard for setup, asset creation, database browsing, validation, samples, and authoring documentation.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
        }

        private void DrawQuickStart()
        {
            EditorGUILayout.LabelField("Quick Start", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("1. Validate project setup. 2. Create content assets. 3. Use database search to find duplicates. 4. Open focused tools for quests, dialogue, saves, and world state.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Authoring Folder") && !AssetDatabase.IsValidFolder(RPGToolkitAuthoringWorkflow.DefaultAssetFolder)) AssetDatabase.CreateFolder("Assets", "RPGToolkit2D");
            if (GUILayout.Button("Open Editor Tools Docs")) RPGToolkitAuthoringWorkflow.OpenDocumentation(RPGToolkitAuthoringWorkflow.EditorToolsDocumentationPath);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawCapabilityLegend()
        {
            EditorGUILayout.LabelField("Dashboard Status Legend", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Complete = production-ready dashboard workflow. Partial = usable but known gaps remain. Missing = planned but not yet implemented. N/A = not required for this content type.", MessageType.Info);
            EditorGUILayout.Space();
        }

        private void DrawCapabilityChips(RPGToolkitDashboardCapability capability)
        {
            EditorGUILayout.BeginHorizontal();
            DrawStatusChip("Create", capability.AssetCreationStatus);
            DrawStatusChip("Editor", capability.FocusedEditorStatus);
            DrawStatusChip("Validate", capability.ValidationStatus);
            DrawStatusChip("Docs", capability.DocumentationStatus);
            DrawStatusChip("Runtime", capability.RuntimeIntegrationStatus);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusChip(string label, RPGToolkitDashboardCapabilityStatus status)
        {
            var previousColor = GUI.backgroundColor;
            GUI.backgroundColor = StatusColor(status);
            GUILayout.Label($"{label}: {StatusLabel(status)}", EditorStyles.miniButton, GUILayout.Width(112f));
            GUI.backgroundColor = previousColor;
        }

        private static string StatusLabel(RPGToolkitDashboardCapabilityStatus status)
        {
            switch (status)
            {
                case RPGToolkitDashboardCapabilityStatus.Complete: return "Complete";
                case RPGToolkitDashboardCapabilityStatus.Partial: return "Partial";
                case RPGToolkitDashboardCapabilityStatus.Missing: return "Missing";
                case RPGToolkitDashboardCapabilityStatus.NotApplicable: return "N/A";
                default: return status.ToString();
            }
        }

        private static Color StatusColor(RPGToolkitDashboardCapabilityStatus status)
        {
            switch (status)
            {
                case RPGToolkitDashboardCapabilityStatus.Complete: return new Color(0.55f, 0.9f, 0.55f);
                case RPGToolkitDashboardCapabilityStatus.Partial: return new Color(1f, 0.85f, 0.35f);
                case RPGToolkitDashboardCapabilityStatus.Missing: return new Color(1f, 0.55f, 0.5f);
                case RPGToolkitDashboardCapabilityStatus.NotApplicable: return new Color(0.75f, 0.75f, 0.75f);
                default: return Color.white;
            }
        }

        private void DrawAuthoringSections()
        {
            var compact = position.width < 760f;
            EditorGUILayout.LabelField("Content Workflow Cards", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(compact ? "Compact mode is active: card details are condensed for this window width." : "Expand cards to review project counts, validation health, setup hints, and next actions.", EditorStyles.wordWrappedMiniLabel);
            foreach (var section in RPGToolkitAuthoringWorkflow.Sections)
            {
                var data = RPGToolkitAuthoringWorkflow.BuildCardData(section, _lastValidationResults);
                if (!_expandedCards.ContainsKey(section.Title)) _expandedCards[section.Title] = !compact;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                _expandedCards[section.Title] = EditorGUILayout.Foldout(_expandedCards[section.Title], section.Title, true);
                GUILayout.FlexibleSpace();
                DrawCountPill($"{data.AssetCount} asset(s)");
                if (data.DuplicateIdCount > 0) DrawCountPill($"{data.DuplicateIdCount} duplicate ID");
                if (data.InvalidAssetCount > 0) DrawCountPill($"{data.InvalidAssetCount} invalid");
                EditorGUILayout.EndHorizontal();

                if (!compact) EditorGUILayout.LabelField(section.Description, EditorStyles.wordWrappedLabel);
                DrawCapabilityChips(section.Capability);

                if (_expandedCards[section.Title])
                {
                    EditorGUILayout.LabelField(section.Description, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.LabelField($"Validation: {data.LastValidationSummary}", EditorStyles.wordWrappedMiniLabel);
                    if (!string.IsNullOrWhiteSpace(data.EmptyContentWarning)) EditorGUILayout.HelpBox(data.EmptyContentWarning, MessageType.Warning);
                    if (data.DuplicateIdCount > 0) EditorGUILayout.HelpBox($"{data.DuplicateIdCount} {section.Title} asset(s) share a duplicate RPG ID.", MessageType.Warning);
                    if (data.InvalidAssetCount > 0) EditorGUILayout.HelpBox($"{data.InvalidAssetCount} {section.Title} asset(s) failed content validation.", MessageType.Warning);
                    if (!string.IsNullOrWhiteSpace(section.SetupHint)) EditorGUILayout.HelpBox(section.SetupHint, MessageType.Info);
                    if (!string.IsNullOrWhiteSpace(section.Capability.Notes)) EditorGUILayout.LabelField(section.Capability.Notes, EditorStyles.wordWrappedMiniLabel);
                    if (section.AssetType == typeof(ItemDefinition)) DrawItemCardBreakdown(data);
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Create", $"Create a new {section.AssetType.Name} asset in the default authoring folder."))) RPGToolkitAuthoringWorkflow.CreateAsset(section);
                if (GUILayout.Button(new GUIContent("Browse", $"Show {section.Title} in the database browser."))) SelectDatabaseSection(section);
                if (GUILayout.Button(new GUIContent("Validate", $"Validate {section.Title} dashboard card data."))) RPGToolkitAuthoringWorkflow.ValidateCard(section, _lastValidationResults);
                var toolAvailable = section.Capability.FocusedEditorStatus != RPGToolkitDashboardCapabilityStatus.Missing && RPGToolkitAuthoringWorkflow.HasFocusedTool(section);
                using (new EditorGUI.DisabledScope(!toolAvailable))
                {
                    var toolLabel = toolAvailable ? "Open Tool" : "Tool Unavailable";
                    var toolTip = toolAvailable ? $"Open the focused {section.Title} authoring tool." : $"No focused {section.Title} tool exists yet; use Docs for current guidance.";
                    if (GUILayout.Button(new GUIContent(toolLabel, toolTip))) RPGToolkitAuthoringWorkflow.TryOpenFocusedTool(section);
                }

                if (GUILayout.Button(new GUIContent("Docs", $"Open the {section.Title} authoring documentation."))) RPGToolkitAuthoringWorkflow.TryOpenDocumentation(section);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space();
        }

        private void DrawItemCardBreakdown(RPGToolkitDashboardCardData data)
        {
            var items = data.Entries.Select(entry => entry.Asset as ItemDefinition).Where(item => item != null).ToList();
            if (items.Count == 0) return;
            EditorGUILayout.LabelField("Item types: " + string.Join(", ", items.GroupBy(item => item.ItemType).Select(group => $"{group.Key} {group.Count()}")), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Item rarities: " + string.Join(", ", items.GroupBy(item => item.Rarity).Select(group => $"{group.Key} {group.Count()}")), EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawCountPill(string text)
        {
            GUILayout.Label(text, EditorStyles.miniButton, GUILayout.Width(110f));
        }

        private void SelectDatabaseSection(RPGToolkitAuthoringSection section)
        {
            var sections = RPGToolkitAuthoringWorkflow.Sections;
            for (var i = 0; i < sections.Count; i++)
            {
                if (sections[i] != section) continue;
                _selectedSection = i;
                break;
            }
        }

        private void DrawDatabaseBrowser()
        {
            EditorGUILayout.LabelField("Database Browser", EditorStyles.boldLabel);
            var sections = RPGToolkitAuthoringWorkflow.Sections;
            var names = new string[sections.Count];
            for (var i = 0; i < sections.Count; i++) names[i] = sections[i].Title;
            _selectedSection = EditorGUILayout.Popup("Content Type", _selectedSection, names);
            _searchText = EditorGUILayout.TextField("Search", _searchText);
            var entries = RPGToolkitAuthoringWorkflow.FindAssets(sections[_selectedSection], _searchText);
            EditorGUILayout.LabelField($"{entries.Count} asset(s) found", EditorStyles.miniLabel);
            foreach (var entry in entries)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(entry.Asset, entry.Asset.GetType(), false);
                EditorGUILayout.LabelField(entry.DuplicateId ? "Duplicate ID" : entry.Path, entry.DuplicateId ? EditorStyles.boldLabel : EditorStyles.miniLabel);
                if (GUILayout.Button("Ping", GUILayout.Width(60f))) EditorGUIUtility.PingObject(entry.Asset);
                if (GUILayout.Button("Open", GUILayout.Width(60f))) Selection.activeObject = entry.Asset;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
        }

        private void DrawUtilities()
        {
            EditorGUILayout.LabelField("Focused Tools", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Character Editor")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.CharacterEditorWindow.Open();
            if (GUILayout.Button("Quest Editor")) global::SixStringSyn.RPGToolkit2D.Editor.QuestEditor.QuestEditorWindow.Open();
            if (GUILayout.Button("Dialogue Graph")) global::SixStringSyn.RPGToolkit2D.Editor.DialogueGraph.DialogueGraphEditorWindow.Open();
            if (GUILayout.Button("Item Database")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.ItemDatabaseWindow.Open();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Data Debugger")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.SaveDataDebuggerWindow.Open();
            if (GUILayout.Button("World State Debugger")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.WorldStateDebuggerWindow.Open();
            if (GUILayout.Button("Samples Folder")) EditorUtility.RevealInFinder(RPGToolkitPackageValidator.PackagePath + "/Samples~");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Map Editor")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.MapEditorWindow.Open();
            if (GUILayout.Button("Tileset Editor")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.TilesetEditorWindow.Open();
            if (GUILayout.Button("Sprite Sheet Editor")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.SpriteSheetEditorWindow.Open();
            if (GUILayout.Button("Map Connections")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.MapConnectionBrowserWindow.ShowWindow();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawValidation()
        {
            EditorGUILayout.LabelField("Project Setup and Validation", EditorStyles.boldLabel);
            foreach (var result in RPGToolkitAuthoringWorkflow.ValidateProjectSetup())
            {
                var type = result.Passed ? MessageType.Info : MessageType.Warning;
                EditorGUILayout.HelpBox($"{result.RuleName}: {result.Message}", type);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Maps, Tilesets, and Sprite Sheets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate All Map Content")) _mapValidationReport = RPGMapProjectValidationService.ValidateAllMapContent();
            if (GUILayout.Button("Validate Sprite Sheets")) _mapValidationReport = RPGMapProjectValidationService.ValidateSpriteSheets();
            if (GUILayout.Button("Validate Tilesets")) _mapValidationReport = RPGMapProjectValidationService.ValidateTilesets();
            if (GUILayout.Button("Validate Maps")) _mapValidationReport = RPGMapProjectValidationService.ValidateMaps();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate Map Graph")) _mapValidationReport = RPGMapProjectValidationService.ValidateMapGraphConnections();
            if (GUILayout.Button("Report Duplicate IDs")) _mapValidationReport = RPGMapProjectValidationService.ValidateDuplicateAssetIds();
            if (GUILayout.Button("Repair Safe Issues")) EditorUtility.DisplayDialog("RPG Toolkit Repairs", $"Updated {RPGMapProjectValidationService.RepairSafeMapContent()} asset(s).", "OK");
            EditorGUILayout.EndHorizontal();

            if (_mapValidationReport == null) return;
            EditorGUILayout.HelpBox($"Map workflow validation: {_mapValidationReport.ErrorCount} error(s), {_mapValidationReport.WarningCount} warning(s).", _mapValidationReport.Passed ? MessageType.Info : MessageType.Warning);
            foreach (var entry in _mapValidationReport.Entries)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(entry.Asset, typeof(UnityEngine.Object), false);
                EditorGUILayout.HelpBox($"[{entry.Message.Code}] {entry.Message.Message}", entry.Message.IsError ? MessageType.Error : MessageType.Warning);
                if (GUILayout.Button("Ping", GUILayout.Width(60f))) EditorGUIUtility.PingObject(entry.Asset);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
