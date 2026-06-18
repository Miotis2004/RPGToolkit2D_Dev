using SixStringSyn.RPGToolkit2D.Editor;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Dashboard
{
    public sealed class RPGToolkitDashboardWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _searchText = string.Empty;
        private int _selectedSection;

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

        private void DrawAuthoringSections()
        {
            EditorGUILayout.LabelField("Create RPG Content", EditorStyles.boldLabel);
            foreach (var section in RPGToolkitAuthoringWorkflow.Sections)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(section.Title, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(section.Description, EditorStyles.wordWrappedLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"Create {section.Title}")) RPGToolkitAuthoringWorkflow.CreateAsset(section);
                if (GUILayout.Button("Open Menu/Docs")) RPGToolkitAuthoringWorkflow.OpenDocumentation(section.DocumentationPath);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space();
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
            if (GUILayout.Button("Quest Editor")) global::SixStringSyn.RPGToolkit2D.Editor.QuestEditor.QuestEditorWindow.Open();
            if (GUILayout.Button("Dialogue Graph")) global::SixStringSyn.RPGToolkit2D.Editor.DialogueGraph.DialogueGraphEditorWindow.Open();
            if (GUILayout.Button("Item Database")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.ItemDatabaseWindow.Open();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Data Debugger")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.SaveDataDebuggerWindow.Open();
            if (GUILayout.Button("World State Debugger")) global::SixStringSyn.RPGToolkit2D.Editor.Windows.WorldStateDebuggerWindow.Open();
            if (GUILayout.Button("Samples Folder")) EditorUtility.RevealInFinder(RPGToolkitPackageValidator.PackagePath + "/Samples~");
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
        }
    }
}
