using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Editor.Dashboard;
using SixStringSyn.RPGToolkit2D.Editor.DialogueGraph;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using SixStringSyn.RPGToolkit2D.Runtime.NPCs;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class NPCEditorWindow : EditorWindow
    {
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private string _search = string.Empty;
        private NPCDefinition _selected;
        private SerializedObject _serializedNpc;

        [MenuItem("Tools/RPG Toolkit/NPC Editor")]
        public static void Open()
        {
            var window = GetWindow<NPCEditorWindow>("NPCs");
            window.minSize = new Vector2(780f, 500f);
            window.Show();
        }

        public static RPGValidationResult ValidateNpc(NPCDefinition npc, IEnumerable<NPCDefinition> allNpcs = null)
        {
            var result = new RPGValidationResult();
            if (npc == null)
            {
                result.AddError("RPG_NPC_NULL", "NPC definition is missing.");
                return result;
            }

            if (npc.Id.IsEmpty) result.AddError("RPG_NPC_MISSING_ID", "NPC is missing an RPG ID.");
            var serialized = new SerializedObject(npc);
            var authoredDisplayName = serialized.FindProperty("_displayName")?.stringValue;
            if (string.IsNullOrWhiteSpace(authoredDisplayName)) result.AddWarning("RPG_NPC_MISSING_DISPLAY_NAME", "NPC should have an authored display name instead of relying on the asset filename.", npc.Id);
            if (npc.Character == null) result.AddWarning("RPG_NPC_MISSING_CHARACTER", "NPC has no linked Character Definition for portrait, prefab, stats, or party metadata.", npc.Id);
            if (npc.Dialogue == null) result.AddWarning("RPG_NPC_MISSING_DIALOGUE", "NPC has no linked Dialogue Definition, so interactions cannot start a conversation by default.", npc.Id);
            if (npc.CanRecruit && string.IsNullOrWhiteSpace(npc.RecruitmentWorldStateKey)) result.AddWarning("RPG_NPC_RECRUITMENT_KEY_MISSING", "Recruitable NPC should define a recruitment world-state key.", npc.Id);

            var keys = new HashSet<string>();
            foreach (var key in npc.WorldStateKeys)
            {
                if (string.IsNullOrWhiteSpace(key)) result.AddWarning("RPG_NPC_EMPTY_WORLD_STATE_KEY", "NPC contains an empty world-state key entry.", npc.Id);
                else if (!keys.Add(key)) result.AddWarning("RPG_NPC_DUPLICATE_WORLD_STATE_KEY", $"NPC world-state key '{key}' is listed more than once.", npc.Id);
            }

            if (allNpcs != null && !npc.Id.IsEmpty)
            {
                var duplicates = allNpcs.Count(other => other != null && other != npc && other.Id == npc.Id);
                if (duplicates > 0) result.AddError("RPG_NPC_DUPLICATE_ID", $"NPC ID '{npc.Id}' is used by {duplicates + 1} NPC assets.", npc.Id);
            }

            return result;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("NPC Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Create, duplicate, link, validate, and edit NPCDefinition assets for dialogue, schedules, recruitment, and world-state workflows.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            DrawNpcList();
            DrawNpcDetails();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNpcList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(280f));
            _search = EditorGUILayout.TextField("Search", _search);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create")) Select(CreateNpc());
            using (new EditorGUI.DisabledScope(_selected == null)) if (GUILayout.Button("Duplicate")) Select(DuplicateNpc(_selected));
            EditorGUILayout.EndHorizontal();

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, EditorStyles.helpBox);
            foreach (var npc in FindNpcs(_search)) if (GUILayout.Toggle(_selected == npc, npc.DisplayName, EditorStyles.miniButton)) Select(npc);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawNpcDetails()
        {
            EditorGUILayout.BeginVertical();
            if (_selected == null)
            {
                EditorGUILayout.HelpBox("Select or create an NPC to edit metadata, dialogue links, party hooks, schedules, and world-state keys.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            if (_serializedNpc == null || _serializedNpc.targetObject != _selected) _serializedNpc = new SerializedObject(_selected);
            _serializedNpc.Update();
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Asset", _selected, typeof(NPCDefinition), false);
            if (GUILayout.Button("Ping", GUILayout.Width(70f))) EditorGUIUtility.PingObject(_selected);
            if (GUILayout.Button("Select", GUILayout.Width(70f))) Selection.activeObject = _selected;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Metadata", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_id"));
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_displayName"));
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_description"));
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_tags"), true);

            EditorGUILayout.LabelField("Runtime Links", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_character"));
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_dialogue"));
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_vendor"));
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_schedule"));

            EditorGUILayout.LabelField("Party Hooks and World State", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_canRecruit"));
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_recruitmentWorldStateKey"));
            EditorGUILayout.PropertyField(_serializedNpc.FindProperty("_worldStateKeys"), true);
            _serializedNpc.ApplyModifiedProperties();

            DrawRelationshipPanel();
            DrawQuickActions();
            DrawValidationSummary();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRelationshipPanel()
        {
            EditorGUILayout.LabelField("Relationships", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Linked Dialogue", _selected.Dialogue, typeof(DialogueDefinition), false);
            EditorGUILayout.LabelField("Linked Quests", EditorStyles.boldLabel);
            var quests = FindLinkedQuests(_selected).ToList();
            if (quests.Count == 0) EditorGUILayout.HelpBox("No quest objectives currently target this NPC ID.", MessageType.Info);
            foreach (var quest in quests)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.ObjectField(quest, typeof(QuestDefinition), false);
                if (GUILayout.Button("Ping", GUILayout.Width(70f))) EditorGUIUtility.PingObject(quest);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_selected.Dialogue == null))
            {
                if (GUILayout.Button("Open Linked Dialogue Graph"))
                {
                    Selection.activeObject = _selected.Dialogue;
                    EditorGUIUtility.PingObject(_selected.Dialogue);
                    DialogueGraphEditorWindow.Open();
                }
            }
            if (GUILayout.Button("Open World State Debugger")) WorldStateDebuggerWindow.Open();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawValidationSummary()
        {
            var result = ValidateNpc(_selected, FindNpcs(null));
            EditorGUILayout.LabelField("Validation Summary", EditorStyles.boldLabel);
            if (result.Messages.Count == 0) { EditorGUILayout.HelpBox("NPC is valid.", MessageType.Info); return; }
            foreach (var message in result.Messages) EditorGUILayout.HelpBox($"[{message.Code}] {message.Message}", message.IsError ? MessageType.Error : MessageType.Warning);
        }

        private static IReadOnlyList<NPCDefinition> FindNpcs(string search)
        {
            return RPGToolkitAuthoringWorkflow.FindAssets(RPGToolkitAuthoringWorkflow.Sections.First(section => section.AssetType == typeof(NPCDefinition)), search)
                .Select(entry => entry.Asset as NPCDefinition).Where(asset => asset != null).OrderBy(asset => asset.DisplayName).ToList();
        }

        private static IEnumerable<QuestDefinition> FindLinkedQuests(NPCDefinition npc)
        {
            if (npc == null || npc.Id.IsEmpty) yield break;
            var guids = AssetDatabase.FindAssets("t:QuestDefinition");
            foreach (var guid in guids)
            {
                var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (quest != null && quest.Objectives.Any(objective => objective != null && objective.TargetId == npc.Id.Value)) yield return quest;
            }
        }

        private static NPCDefinition CreateNpc()
        {
            var section = RPGToolkitAuthoringWorkflow.Sections.First(s => s.AssetType == typeof(NPCDefinition));
            return RPGToolkitAuthoringWorkflow.CreateAsset(section) as NPCDefinition;
        }

        private static NPCDefinition DuplicateNpc(NPCDefinition source)
        {
            if (source == null) return null;
            var clone = Instantiate(source);
            clone.AssignNewId();
            var sourcePath = AssetDatabase.GetAssetPath(source);
            var folder = string.IsNullOrWhiteSpace(sourcePath) ? RPGToolkitAuthoringWorkflow.DefaultAssetFolder : Path.GetDirectoryName(sourcePath).Replace('\\', '/');
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, source.name + " Copy.asset").Replace('\\', '/'));
            AssetDatabase.CreateAsset(clone, path);
            AssetDatabase.SaveAssets();
            return clone;
        }

        private void Select(NPCDefinition npc)
        {
            _selected = npc;
            _serializedNpc = npc == null ? null : new SerializedObject(npc);
            Selection.activeObject = npc;
        }
    }
}
