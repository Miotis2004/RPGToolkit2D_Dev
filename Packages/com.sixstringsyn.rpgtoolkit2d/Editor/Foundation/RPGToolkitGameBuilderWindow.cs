using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Editor.Dashboard;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Foundation
{
    public sealed class RPGToolkitGameBuilderWindow : EditorWindow
    {
        private const string FavoritesKey = "SixStringSyn.RPGToolkit2D.GameBuilder.Favorites";
        private const string RecentKey = "SixStringSyn.RPGToolkit2D.GameBuilder.Recent";
        private const int MaxTrackedAssets = 12;

        private static readonly IReadOnlyList<string> CoreTabs = new[]
        {
            "Characters", "Items", "Dialogue", "Quests", "Combat", "Maps", "Tilesets", "Sprite Sheets", "NPCs", "Events", "Variables", "Save Data", "Settings"
        };

        [SerializeField] private string _selectedPageId;
        [SerializeField] private string _searchText = string.Empty;
        [SerializeField] private Vector2 _sidebarScroll;
        [SerializeField] private Vector2 _contentScroll;

        public static IReadOnlyList<string> RequiredPhase7Tabs => CoreTabs;

        [MenuItem("Tools/RPG Toolkit")]
        [MenuItem("Tools/RPG Toolkit/Game Builder")]
        [MenuItem("Window/RPG Toolkit/Game Builder")]
        public static void Open()
        {
            var window = GetWindow<RPGToolkitGameBuilderWindow>("RPG Game Builder");
            window.minSize = new Vector2(860f, 560f);
            window.Show();
        }

        public void SelectPage(string pageId)
        {
            _selectedPageId = pageId;
            Repaint();
        }

        private void OnEnable() => RegisterPhase7Pages();

        private static void RegisterPhase7Pages()
        {
            for (var i = 0; i < CoreTabs.Count; i++)
            {
                var tab = CoreTabs[i];
                RPGBuilderNavigation.RegisterPage(new RPGBuilderPage(NormalizePageId(tab), tab, i * 10, () => DrawBuilderTab(tab)));
            }
        }

        private void OnGUI()
        {
            RegisterPhase7Pages();
            DrawToolbar();

            var pages = RPGBuilderNavigation.RegisteredPages;
            if (pages.Count == 0)
            {
                EditorGUILayout.HelpBox("No RPG Toolkit pages are registered.", MessageType.Info);
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedPageId) || !pages.Any(page => page.Id == _selectedPageId)) _selectedPageId = pages[0].Id;

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSidebar(pages);
                DrawSelectedPage(pages);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("RPG Toolkit Game Builder", EditorStyles.boldLabel, GUILayout.Width(210f));
                _searchText = GUILayout.TextField(_searchText, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.MinWidth(220f));
                if (GUILayout.Button("Search", EditorStyles.toolbarButton, GUILayout.Width(64f))) SelectPage("settings");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(72f))) SelectPage("settings");
                if (GUILayout.Button("Docs", EditorStyles.toolbarButton, GUILayout.Width(52f))) RPGToolkitAuthoringWorkflow.OpenDocumentation(RPGToolkitAuthoringWorkflow.EditorToolsDocumentationPath);
            }
        }

        private void DrawSidebar(IReadOnlyList<RPGBuilderPage> pages)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(205f)))
            {
                _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);
                EditorGUILayout.LabelField("Authoring Tabs", EditorStyles.boldLabel);
                foreach (var page in pages)
                {
                    if (GUILayout.Toggle(_selectedPageId == page.Id, page.Title, "Button")) _selectedPageId = page.Id;
                }

                EditorGUILayout.Space();
                DrawTrackedAssets("Recent Assets", LoadTrackedPaths(RecentKey), false);
                DrawTrackedAssets("Favorites", LoadTrackedPaths(FavoritesKey), true);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSelectedPage(IReadOnlyList<RPGBuilderPage> pages)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);
                DrawGlobalSearchResults();
                var page = pages.FirstOrDefault(candidate => candidate.Id == _selectedPageId);
                page.Draw?.Invoke();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawGlobalSearchResults()
        {
            if (string.IsNullOrWhiteSpace(_searchText)) return;
            EditorGUILayout.LabelField("Cross-System Search", EditorStyles.boldLabel);
            var total = 0;
            foreach (var section in RPGToolkitAuthoringWorkflow.Sections)
            {
                var entries = RPGToolkitAuthoringWorkflow.FindAssets(section, _searchText);
                if (entries.Count == 0) continue;
                total += entries.Count;
                EditorGUILayout.LabelField(section.Title, EditorStyles.miniBoldLabel);
                foreach (var entry in entries.Take(5)) DrawAssetRow(entry.Asset, entry.Path, entry.DuplicateId);
            }
            if (total == 0) EditorGUILayout.HelpBox($"No RPG Toolkit assets matched '{_searchText}'.", MessageType.Info);
            EditorGUILayout.Space();
        }

        private static void DrawBuilderTab(string tab)
        {
            EditorGUILayout.LabelField(tab, EditorStyles.boldLabel);
            var section = RPGToolkitAuthoringWorkflow.Sections.FirstOrDefault(candidate => string.Equals(candidate.Title, tab, StringComparison.OrdinalIgnoreCase));
            if (section != null)
            {
                EditorGUILayout.HelpBox(section.Description, MessageType.Info);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button($"Create {section.Title}")) TrackSelection(RPGToolkitAuthoringWorkflow.CreateAsset(section));
                    if (GUILayout.Button("Open Documentation")) RPGToolkitAuthoringWorkflow.OpenDocumentation(section.DocumentationPath);
                }
                foreach (var entry in RPGToolkitAuthoringWorkflow.FindAssets(section).Take(25)) DrawAssetRow(entry.Asset, entry.Path, entry.DuplicateId);
                return;
            }

            DrawUtilityTab(tab);
        }

        private static void DrawUtilityTab(string tab)
        {
            switch (tab)
            {
                case "Combat":
                    EditorGUILayout.HelpBox("Combat content is assembled from abilities, loot tables, enemies, factions, and behavior trees.", MessageType.Info);
                    if (GUILayout.Button("Open Behavior Tree Editor")) global::SixStringSyn.RPGToolkit2D.Editor.AI.BehaviorTreeEditorWindow.Open();
                    break;
                case "Events":
                    if (GUILayout.Button("Open Event Editor")) global::SixStringSyn.RPGToolkit2D.Editor.EventEditor.RPGEventEditorWindow.Open();
                    break;
                case "Save Data":
                    if (GUILayout.Button("Open Save Data Debugger")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.SaveDataDebuggerWindow.Open();
                    break;
                case "Variables":
                    if (GUILayout.Button("Open World State Debugger")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.WorldStateDebuggerWindow.Open();
                    break;
                case "Settings":
                    DrawValidationDashboard();
                    break;
                default:
                    EditorGUILayout.HelpBox($"Use this workspace to collect, search, validate, and create {tab.ToLowerInvariant()} content as project-specific assets are added.", MessageType.Info);
                    break;
            }
        }

        private static void DrawValidationDashboard()
        {
            EditorGUILayout.LabelField("Project Setup Checklist and Validation Dashboard", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Authoring Folder") && !AssetDatabase.IsValidFolder(RPGToolkitAuthoringWorkflow.DefaultAssetFolder)) AssetDatabase.CreateFolder("Assets", "RPGToolkit2D");
            foreach (var result in RPGToolkitAuthoringWorkflow.ValidateProjectSetup())
            {
                EditorGUILayout.HelpBox($"{result.RuleName}: {result.Message}", result.Passed ? MessageType.Info : MessageType.Warning);
            }
        }

        private static void DrawAssetRow(UnityEngine.Object asset, string path, bool duplicateId)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.ObjectField(asset, asset.GetType(), false);
                EditorGUILayout.LabelField(duplicateId ? $"Duplicate ID - {path}" : path, duplicateId ? EditorStyles.boldLabel : EditorStyles.miniLabel);
                if (GUILayout.Button("★", GUILayout.Width(28f))) ToggleFavorite(path);
                if (GUILayout.Button("Open", GUILayout.Width(58f))) { Selection.activeObject = asset; TrackSelection(asset); }
                if (GUILayout.Button("Ping", GUILayout.Width(52f))) EditorGUIUtility.PingObject(asset);
            }
        }

        private static void DrawTrackedAssets(string title, IReadOnlyList<string> paths, bool favorites)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (paths.Count == 0) { EditorGUILayout.LabelField("None yet", EditorStyles.miniLabel); return; }
            foreach (var path in paths.Take(6))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(System.IO.Path.GetFileNameWithoutExtension(path), EditorStyles.miniButton)) TrackSelection(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
                    if (favorites && GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(22f))) ToggleFavorite(path);
                }
            }
        }

        private static void TrackSelection(UnityEngine.Object asset)
        {
            if (asset == null) return;
            Selection.activeObject = asset;
            var path = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrWhiteSpace(path)) SaveTrackedPath(RecentKey, path);
        }

        private static void ToggleFavorite(string path)
        {
            var paths = LoadTrackedPaths(FavoritesKey).ToList();
            if (!paths.Remove(path)) paths.Insert(0, path);
            SaveTrackedPaths(FavoritesKey, paths);
        }

        private static void SaveTrackedPath(string key, string path)
        {
            var paths = LoadTrackedPaths(key).Where(existing => existing != path).ToList();
            paths.Insert(0, path);
            SaveTrackedPaths(key, paths);
        }

        private static IReadOnlyList<string> LoadTrackedPaths(string key) => EditorPrefs.GetString(key, string.Empty).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Where(path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null).Take(MaxTrackedAssets).ToList();
        private static void SaveTrackedPaths(string key, IReadOnlyList<string> paths) => EditorPrefs.SetString(key, string.Join("|", paths.Take(MaxTrackedAssets)));
        private static string NormalizePageId(string title) => title.ToLowerInvariant().Replace(" ", "-");
    }
}
