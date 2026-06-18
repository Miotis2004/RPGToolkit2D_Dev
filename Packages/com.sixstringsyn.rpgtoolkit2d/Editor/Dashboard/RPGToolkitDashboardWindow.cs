using SixStringSyn.RPGToolkit2D.Editor;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Dashboard
{
    public enum RPGToolkitDashboardTab
    {
        Overview,
        Create,
        Database,
        Validation,
        Tools,
        DocsSamples
    }

    public sealed class RPGToolkitDashboardWindow : EditorWindow
    {
        public static readonly string[] RequiredPhase11Tabs = { "Overview", "Create", "Database", "Validation", "Tools", "Docs/Samples" };
        private const string SelectedTabPrefsKey = "SixStringSyn.RPGToolkit2D.Dashboard.SelectedTab";
        private const string FavoriteWorkflowsPrefsKey = "SixStringSyn.RPGToolkit2D.Dashboard.Favorites";
        private const string RecentWorkflowsPrefsKey = "SixStringSyn.RPGToolkit2D.Dashboard.Recent";
        private const int MaxRecentWorkflows = 6;

        private Vector2 _scroll;
        private string _searchText = string.Empty;
        private int _selectedSection;
        private RPGToolkitDashboardTab _selectedTab = RPGToolkitDashboardTab.Overview;
        private RPGMapProjectValidationReport _mapValidationReport;
        private RPGToolkitValidationReport _validationCenterReport;
        private string _validationCenterSearch = string.Empty;
        private bool _validationCenterShowErrors = true;
        private bool _validationCenterShowWarnings = true;
        private bool _validationCenterShowInfo;
        private readonly Dictionary<string, bool> _expandedCards = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> _lastValidationResults = new Dictionary<string, string>();
        private List<string> _favoriteWorkflows = new List<string>();
        private List<string> _recentWorkflows = new List<string>();

        [MenuItem("Tools/RPG Toolkit/Dashboard")]
        public static void Open()
        {
            var window = GetWindow<RPGToolkitDashboardWindow>("RPG Toolkit");
            window.minSize = new Vector2(680f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            _selectedTab = (RPGToolkitDashboardTab)EditorPrefs.GetInt(SelectedTabPrefsKey, (int)RPGToolkitDashboardTab.Overview);
            _favoriteWorkflows = LoadWorkflowList(FavoriteWorkflowsPrefsKey);
            _recentWorkflows = LoadWorkflowList(RecentWorkflowsPrefsKey);
        }

        private void OnGUI()
        {
            HandleKeyboardShortcuts();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawHeader();
            DrawTabNavigation();
            DrawSelectedTab();
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("RPG Toolkit 2D", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Central dashboard for setup, asset creation, database browsing, validation, samples, and authoring documentation.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
        }

        private void DrawTabNavigation()
        {
            var selected = GUILayout.Toolbar((int)_selectedTab, RequiredPhase11Tabs);
            if (selected != (int)_selectedTab)
            {
                _selectedTab = (RPGToolkitDashboardTab)selected;
                EditorPrefs.SetInt(SelectedTabPrefsKey, selected);
                GUI.FocusControl(null);
            }
            EditorGUILayout.Space();
        }

        private void DrawSelectedTab()
        {
            switch (_selectedTab)
            {
                case RPGToolkitDashboardTab.Overview:
                    DrawOverview();
                    break;
                case RPGToolkitDashboardTab.Create:
                    DrawQuickStart();
                    DrawCapabilityLegend();
                    DrawAuthoringSections();
                    break;
                case RPGToolkitDashboardTab.Database:
                    DrawDatabaseBrowser();
                    break;
                case RPGToolkitDashboardTab.Validation:
                    DrawValidation();
                    DrawValidationCenter();
                    break;
                case RPGToolkitDashboardTab.Tools:
                    DrawUtilities();
                    break;
                case RPGToolkitDashboardTab.DocsSamples:
                    DrawDocsAndSamples();
                    break;
            }
        }

        private void DrawOverview()
        {
            DrawProjectHealthSummary();
            DrawRecentlyUsedWorkflows();
            DrawFavoriteWorkflows();
            DrawEmptyStateHelp();
            DrawQuickStart();
        }

        private void DrawProjectHealthSummary()
        {
            var cardData = RPGToolkitAuthoringWorkflow.Sections.Select(section => RPGToolkitAuthoringWorkflow.BuildCardData(section, _lastValidationResults)).ToList();
            var assetCount = cardData.Sum(data => data.AssetCount);
            var duplicateCount = cardData.Sum(data => data.DuplicateIdCount);
            var invalidCount = cardData.Sum(data => data.InvalidAssetCount);
            var importantEmptyCount = cardData.Count(data => !string.IsNullOrWhiteSpace(data.EmptyContentWarning));
            var messageType = invalidCount > 0 || duplicateCount > 0 ? MessageType.Warning : assetCount == 0 ? MessageType.Info : MessageType.None;

            EditorGUILayout.LabelField("Project Health", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"{HealthIcon(invalidCount, duplicateCount)} {assetCount} RPG asset(s), {invalidCount} invalid asset(s), {duplicateCount} duplicate ID flag(s), {importantEmptyCount} important empty workflow(s).", messageType);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Refresh Counts", "Clear cached asset queries and rebuild dashboard counts."))) RPGToolkitAuthoringWorkflow.ClearAssetQueryCache();
            if (GUILayout.Button(new GUIContent("Validate All", "Run the Validation Center for every supported RPG content type.")))
            {
                _selectedTab = RPGToolkitDashboardTab.Validation;
                EditorPrefs.SetInt(SelectedTabPrefsKey, (int)_selectedTab);
                _validationCenterReport = RPGToolkitValidationCenter.ValidateAllRPGContent();
            }
            EditorGUILayout.EndHorizontal();
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
                if (data.QuestMissingObjectiveCount > 0) DrawCountPill($"{data.QuestMissingObjectiveCount} no objectives");
                if (data.QuestMissingRewardCount > 0) DrawCountPill($"{data.QuestMissingRewardCount} no rewards");
                if (data.DialogueUnreachableNodeWarningCount > 0) DrawCountPill($"{data.DialogueUnreachableNodeWarningCount} unreachable");
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
                    if (data.QuestMissingObjectiveCount > 0) EditorGUILayout.HelpBox($"{data.QuestMissingObjectiveCount} quest(s) are missing objectives.", MessageType.Warning);
                    if (data.QuestMissingRewardCount > 0) EditorGUILayout.HelpBox($"{data.QuestMissingRewardCount} quest(s) are missing rewards.", MessageType.Warning);
                    if (data.DialogueUnreachableNodeWarningCount > 0) EditorGUILayout.HelpBox($"{data.DialogueUnreachableNodeWarningCount} dialogue node(s) are unreachable from their entry nodes.", MessageType.Warning);
                    if (!string.IsNullOrWhiteSpace(section.SetupHint)) EditorGUILayout.HelpBox(section.SetupHint, MessageType.Info);
                    if (!string.IsNullOrWhiteSpace(section.Capability.Notes)) EditorGUILayout.LabelField(section.Capability.Notes, EditorStyles.wordWrappedMiniLabel);
                    if (section.AssetType == typeof(ItemDefinition)) DrawItemCardBreakdown(data);
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("★", IsFavorite(section.Title) ? "Remove this workflow from favorites." : "Pin this workflow to favorites."), GUILayout.Width(32f))) ToggleFavorite(section.Title);
                if (GUILayout.Button(new GUIContent("Create", $"Create a new {section.AssetType.Name} asset in the default authoring folder."))) { RPGToolkitAuthoringWorkflow.CreateAsset(section); TrackRecent(section.Title); }
                if (GUILayout.Button(new GUIContent("Browse", $"Show {section.Title} in the database browser."))) { SelectDatabaseSection(section); TrackRecent(section.Title); }
                if (GUILayout.Button(new GUIContent("Validate", $"Validate {section.Title} dashboard card data."))) { RPGToolkitAuthoringWorkflow.ValidateCard(section, _lastValidationResults); TrackRecent(section.Title); }
                var toolAvailable = section.Capability.FocusedEditorStatus != RPGToolkitDashboardCapabilityStatus.Missing && RPGToolkitAuthoringWorkflow.HasFocusedTool(section);
                using (new EditorGUI.DisabledScope(!toolAvailable))
                {
                    var toolLabel = toolAvailable ? "Open Tool" : "Tool Unavailable";
                    var toolTip = toolAvailable ? $"Open the focused {section.Title} authoring tool." : $"No focused {section.Title} tool exists yet; use Docs for current guidance.";
                    if (GUILayout.Button(new GUIContent(toolLabel, toolTip))) { RPGToolkitAuthoringWorkflow.TryOpenFocusedTool(section); TrackRecent(section.Title); }
                }

                if (GUILayout.Button(new GUIContent("Docs", $"Open the {section.Title} authoring documentation."))) { RPGToolkitAuthoringWorkflow.TryOpenDocumentation(section); TrackRecent(section.Title); }
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
            _selectedTab = RPGToolkitDashboardTab.Database;
            EditorPrefs.SetInt(SelectedTabPrefsKey, (int)_selectedTab);
        }

        private void DrawDatabaseBrowser()
        {
            EditorGUILayout.LabelField("Database Browser", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Tip: press Ctrl/Cmd+F to focus search. Use Refresh to clear the dashboard asset-query cache for very large projects.", EditorStyles.wordWrappedMiniLabel);
            var sections = RPGToolkitAuthoringWorkflow.Sections;
            var names = new string[sections.Count];
            for (var i = 0; i < sections.Count; i++) names[i] = sections[i].Title;
            _selectedSection = EditorGUILayout.Popup("Content Type", _selectedSection, names);
            GUI.SetNextControlName("RPGToolkitDashboardSearch");
            _searchText = EditorGUILayout.TextField("Search", _searchText);
            if (GUILayout.Button("Refresh Cached Queries")) RPGToolkitAuthoringWorkflow.ClearAssetQueryCache();
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

        private void DrawDocsAndSamples()
        {
            EditorGUILayout.LabelField("Docs and Samples", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Open authoring guides, samples, and content-specific documentation from one place.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Editor Tools Docs")) RPGToolkitAuthoringWorkflow.OpenDocumentation(RPGToolkitAuthoringWorkflow.EditorToolsDocumentationPath);
            if (GUILayout.Button("Package Samples")) EditorUtility.RevealInFinder(RPGToolkitPackageValidator.PackagePath + "/Samples~");
            if (GUILayout.Button("Package README")) RPGToolkitAuthoringWorkflow.OpenDocumentation(RPGToolkitPackageValidator.PackagePath + "/README.md");
            EditorGUILayout.EndHorizontal();
            foreach (var section in RPGToolkitAuthoringWorkflow.Sections)
            {
                if (GUILayout.Button(new GUIContent(section.Title + " Docs", section.DocumentationPath))) RPGToolkitAuthoringWorkflow.TryOpenDocumentation(section);
            }
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

        private void DrawValidationCenter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation Center", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Validate every supported RPG content type, filter diagnostics by severity or text, jump to affected assets, preview deterministic repairs, and export a Markdown report.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate All RPG Content"))
            {
                _validationCenterReport = RPGToolkitValidationCenter.ValidateAllRPGContent();
                _lastValidationResults["Validation Center"] = $"{System.DateTime.Now:HH:mm:ss}: {_validationCenterReport.ErrorCount} error(s), {_validationCenterReport.WarningCount} warning(s), {_validationCenterReport.InfoCount} info message(s).";
            }

            using (new EditorGUI.DisabledScope(_validationCenterReport == null))
            {
                if (GUILayout.Button("Copy Markdown Report")) EditorGUIUtility.systemCopyBuffer = RPGToolkitValidationCenter.ExportMarkdown(_validationCenterReport);
                if (GUILayout.Button("Repair All Safe Issues"))
                {
                    var repairCount = RPGToolkitValidationCenter.RepairAllSafe(_validationCenterReport);
                    EditorUtility.DisplayDialog("RPG Toolkit Repairs", $"Applied {repairCount} safe repair(s). Re-run validation to refresh diagnostics.", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _validationCenterShowErrors = EditorGUILayout.ToggleLeft("Errors", _validationCenterShowErrors, GUILayout.Width(80f));
            _validationCenterShowWarnings = EditorGUILayout.ToggleLeft("Warnings", _validationCenterShowWarnings, GUILayout.Width(100f));
            _validationCenterShowInfo = EditorGUILayout.ToggleLeft("Info", _validationCenterShowInfo, GUILayout.Width(80f));
            GUILayout.Label("Search", GUILayout.Width(48f));
            _validationCenterSearch = EditorGUILayout.TextField(_validationCenterSearch);
            EditorGUILayout.EndHorizontal();

            if (_validationCenterReport == null)
            {
                EditorGUILayout.LabelField("No validation report has been generated this editor session.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

            EditorGUILayout.HelpBox($"Validation Center: {_validationCenterReport.ErrorCount} error(s), {_validationCenterReport.WarningCount} warning(s), {_validationCenterReport.InfoCount} info message(s).", _validationCenterReport.Passed ? MessageType.Info : MessageType.Warning);
            var diagnostics = RPGToolkitValidationCenter.Filter(_validationCenterReport, _validationCenterShowErrors, _validationCenterShowWarnings, _validationCenterShowInfo, _validationCenterSearch);
            foreach (var group in diagnostics.GroupBy(diagnostic => diagnostic.Section.Title))
            {
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                foreach (var diagnostic in group)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(diagnostic.Asset, typeof(UnityEngine.Object), false);
                    if (GUILayout.Button("Ping", GUILayout.Width(56f)) && diagnostic.Asset != null) EditorGUIUtility.PingObject(diagnostic.Asset);
                    if (GUILayout.Button("Select", GUILayout.Width(60f)) && diagnostic.Asset != null) Selection.activeObject = diagnostic.Asset;
                    if (GUILayout.Button("Open", GUILayout.Width(56f)) && diagnostic.Asset != null) AssetDatabase.OpenAsset(diagnostic.Asset);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox($"[{diagnostic.Message.Code}] {diagnostic.Message.Message}", ToMessageType(diagnostic.Message.Severity));
                    if (diagnostic.HasRepair)
                    {
                        EditorGUILayout.LabelField("Repair preview: " + diagnostic.RepairPreview, EditorStyles.wordWrappedMiniLabel);
                        if (GUILayout.Button("Apply Safe Repair"))
                        {
                            diagnostic.RepairAction.Invoke();
                            AssetDatabase.SaveAssets();
                            EditorUtility.DisplayDialog("RPG Toolkit Repair", "Applied safe repair. Re-run validation to refresh diagnostics.", "OK");
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void DrawRecentlyUsedWorkflows()
        {
            EditorGUILayout.LabelField("Recently Used", EditorStyles.boldLabel);
            if (_recentWorkflows.Count == 0)
            {
                EditorGUILayout.LabelField("No recent dashboard workflows yet.", EditorStyles.wordWrappedMiniLabel);
                return;
            }
            DrawWorkflowButtons(_recentWorkflows);
        }

        private void DrawFavoriteWorkflows()
        {
            EditorGUILayout.LabelField("Favorites", EditorStyles.boldLabel);
            if (_favoriteWorkflows.Count == 0)
            {
                EditorGUILayout.LabelField("Pin workflows from the Create tab with the ★ button.", EditorStyles.wordWrappedMiniLabel);
                return;
            }
            DrawWorkflowButtons(_favoriteWorkflows);
        }

        private void DrawWorkflowButtons(IEnumerable<string> workflowTitles)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var title in workflowTitles)
            {
                if (GUILayout.Button(title))
                {
                    var section = RPGToolkitAuthoringWorkflow.Sections.FirstOrDefault(candidate => candidate.Title == title);
                    if (section != null) SelectDatabaseSection(section);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyStateHelp()
        {
            if (RPGToolkitAuthoringWorkflow.Sections.Any(section => RPGToolkitAuthoringWorkflow.FindAssets(section).Count > 0)) return;
            EditorGUILayout.HelpBox("New project empty state: create the authoring folder, add an Item or Character, then run validation to learn what each workflow needs next.", MessageType.Info);
        }

        private void HandleKeyboardShortcuts()
        {
            var current = Event.current;
            if (current == null || current.type != EventType.KeyDown) return;
            if ((current.control || current.command) && current.keyCode == KeyCode.F)
            {
                _selectedTab = RPGToolkitDashboardTab.Database;
                EditorPrefs.SetInt(SelectedTabPrefsKey, (int)_selectedTab);
                EditorApplication.delayCall += () => EditorGUI.FocusTextInControl("RPGToolkitDashboardSearch");
                current.Use();
            }
            else if ((current.control || current.command) && current.keyCode == KeyCode.Return)
            {
                _selectedTab = RPGToolkitDashboardTab.Validation;
                EditorPrefs.SetInt(SelectedTabPrefsKey, (int)_selectedTab);
                _validationCenterReport = RPGToolkitValidationCenter.ValidateAllRPGContent();
                current.Use();
            }
        }

        private static string HealthIcon(int invalidCount, int duplicateCount) => invalidCount > 0 ? "❌" : duplicateCount > 0 ? "⚠" : "✓";

        private bool IsFavorite(string title) => _favoriteWorkflows.Contains(title);

        private void ToggleFavorite(string title)
        {
            if (_favoriteWorkflows.Contains(title)) _favoriteWorkflows.Remove(title);
            else _favoriteWorkflows.Add(title);
            SaveWorkflowList(FavoriteWorkflowsPrefsKey, _favoriteWorkflows);
        }

        private void TrackRecent(string title)
        {
            _recentWorkflows.Remove(title);
            _recentWorkflows.Insert(0, title);
            if (_recentWorkflows.Count > MaxRecentWorkflows) _recentWorkflows.RemoveRange(MaxRecentWorkflows, _recentWorkflows.Count - MaxRecentWorkflows);
            SaveWorkflowList(RecentWorkflowsPrefsKey, _recentWorkflows);
        }

        private static List<string> LoadWorkflowList(string key) => EditorPrefs.GetString(key, string.Empty).Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

        private static void SaveWorkflowList(string key, IEnumerable<string> titles) => EditorPrefs.SetString(key, string.Join("|", titles));

        private static MessageType ToMessageType(SixStringSyn.RPGToolkit2D.Runtime.Core.RPGValidationSeverity severity)
        {
            switch (severity)
            {
                case SixStringSyn.RPGToolkit2D.Runtime.Core.RPGValidationSeverity.Error: return MessageType.Error;
                case SixStringSyn.RPGToolkit2D.Runtime.Core.RPGValidationSeverity.Warning: return MessageType.Warning;
                default: return MessageType.Info;
            }
        }

    }
}
