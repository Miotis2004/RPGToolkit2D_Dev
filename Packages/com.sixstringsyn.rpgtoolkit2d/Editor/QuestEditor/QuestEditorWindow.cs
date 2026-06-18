using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.NPCs;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.QuestEditor
{
    public sealed class QuestEditorWindow : EditorWindow
    {
        [SerializeField] private QuestDefinition _quest;
        [SerializeField] private string _search = string.Empty;
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private SerializedObject _serializedQuest;
        private ReorderableList _objectives;
        private ReorderableList _conditions;
        private ReorderableList _rewards;

        [MenuItem("Tools/RPG Toolkit/Quest Editor")]
        public static void Open() => GetWindow<QuestEditorWindow>("Quest Editor");

        public static QuestDefinition CreateQuestAsset(string folder, string fileName = "NewQuestDefinition.asset")
        {
            if (!AssetDatabase.IsValidFolder(folder)) System.IO.Directory.CreateDirectory(folder);
            var quest = CreateInstance<QuestDefinition>();
            var path = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(folder, fileName).Replace('\\', '/'));
            AssetDatabase.CreateAsset(quest, path);
            AssetDatabase.SaveAssets();
            return quest;
        }

        public static QuestObjectiveDefinition AddObjective(QuestDefinition quest, QuestObjectiveType type, string description, string targetId = null, int requiredAmount = 1)
        {
            if (quest == null) throw new ArgumentNullException(nameof(quest));
            var objective = QuestObjectiveDefinition.Create(type, description, targetId, requiredAmount);
            quest.AddObjective(objective);
            EditorUtility.SetDirty(quest);
            return objective;
        }

        public static QuestRewardDefinition AddReward(QuestDefinition quest, QuestRewardType type, ItemDefinition item = null, int quantity = 1, string currencyId = "gold", string customAction = null)
        {
            if (quest == null) throw new ArgumentNullException(nameof(quest));
            var reward = new QuestRewardDefinition();
            var boxed = new SerializedObject(quest);
            quest.AddReward(reward);
            boxed.Update();
            var rewards = boxed.FindProperty("_rewards");
            var element = rewards.GetArrayElementAtIndex(rewards.arraySize - 1);
            element.FindPropertyRelative("_type").enumValueIndex = (int)type;
            element.FindPropertyRelative("_item").objectReferenceValue = item;
            element.FindPropertyRelative("_quantity").intValue = Mathf.Max(1, quantity);
            element.FindPropertyRelative("_currencyId").stringValue = currencyId ?? string.Empty;
            element.FindPropertyRelative("_customAction").stringValue = customAction ?? string.Empty;
            boxed.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(quest);
            return reward;
        }

        public static RPGValidationResult ValidateQuestWorkflow(QuestDefinition quest) => quest != null ? quest.ValidateQuest() : new RPGValidationResult();

        public static int RepairSafeIssues(QuestDefinition quest)
        {
            if (quest == null || !quest.Id.IsEmpty) return 0;
            quest.AssignNewId();
            EditorUtility.SetDirty(quest);
            return 1;
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawQuestListPanel();
                DrawQuestDetailPanel();
            }
        }

        private void DrawQuestListPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Min(320f, position.width * 0.35f))))
            {
                EditorGUILayout.LabelField("Quest Library", EditorStyles.boldLabel);
                _search = EditorGUILayout.TextField("Search", _search);
                if (GUILayout.Button("Create Quest Asset")) CreateAsset();
                _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
                foreach (var quest in FindQuests(_search))
                {
                    var selected = quest == _quest;
                    if (GUILayout.Toggle(selected, new GUIContent(quest.DisplayName, AssetDatabase.GetAssetPath(quest)), EditorStyles.miniButton) != selected) SelectQuest(quest);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawQuestDetailPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (_quest == null) { EditorGUILayout.HelpBox("Select or create a quest to edit metadata, objectives, conditions, rewards, links, and validation.", MessageType.Info); return; }
                EnsureSerializedQuest();
                _serializedQuest.Update();
                _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
                DrawToolbar();
                DrawMetadata();
                DrawList(_objectives, "Objectives");
                DrawList(_conditions, "Start Conditions / Dependencies");
                DrawList(_rewards, "Rewards");
                EditorGUILayout.PropertyField(_serializedQuest.FindProperty("_autoTurnIn"), new GUIContent("Auto Turn In"));
                _serializedQuest.ApplyModifiedProperties();
                DrawQuickLinks();
                DrawValidationPanel();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField(_quest, typeof(QuestDefinition), false);
                if (GUILayout.Button("Select", GUILayout.Width(70f))) Selection.activeObject = _quest;
                if (GUILayout.Button("Ping", GUILayout.Width(60f))) EditorGUIUtility.PingObject(_quest);
                if (GUILayout.Button("Repair", GUILayout.Width(70f))) RepairSafeIssues(_quest);
            }
        }

        private void DrawMetadata()
        {
            EditorGUILayout.LabelField("Metadata", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedQuest.FindProperty("_id"));
            EditorGUILayout.PropertyField(_serializedQuest.FindProperty("_displayName"));
            EditorGUILayout.PropertyField(_serializedQuest.FindProperty("_description"));
            EditorGUILayout.PropertyField(_serializedQuest.FindProperty("_tags"), true);
        }

        private void DrawList(ReorderableList list, string title)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            list.DoLayoutList();
        }

        private void DrawQuickLinks()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Links", EditorStyles.boldLabel);
            var targetIds = _quest.Objectives.Select(o => o.TargetId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            EditorGUILayout.LabelField("World State / Target Keys: " + (targetIds.Count == 0 ? "None" : string.Join(", ", targetIds)), EditorStyles.wordWrappedMiniLabel);
            DrawLinkedAssets<NPCDefinition>("NPCs", npc => targetIds.Contains(npc.Id.Value));
            DrawLinkedAssets<ItemDefinition>("Items", item => _quest.Objectives.Any(o => o.Item == item) || _quest.Rewards.Any(r => r.Item == item));
            DrawLinkedAssets<DialogueDefinition>("Dialogue", dialogue => dialogue.Nodes.Any(node => node.Commands.Any(command => command.Name.IndexOf("quest", StringComparison.OrdinalIgnoreCase) >= 0)));
        }

        private void DrawValidationPanel()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            var result = _quest.ValidateQuest();
            if (result.Messages.Count == 0) EditorGUILayout.HelpBox("Quest is valid.", MessageType.Info);
            foreach (var group in result.Messages.GroupBy(m => m.Severity).OrderByDescending(g => g.Key))
                foreach (var message in group) EditorGUILayout.HelpBox($"[{message.Code}] {message.Message}", message.IsError ? MessageType.Error : MessageType.Warning);
        }

        private void EnsureSerializedQuest()
        {
            if (_serializedQuest != null && _serializedQuest.targetObject == _quest) return;
            _serializedQuest = new SerializedObject(_quest);
            _objectives = CreateList(_serializedQuest.FindProperty("_objectives"), "Objective", 7f);
            _conditions = CreateList(_serializedQuest.FindProperty("_conditions"), "Condition", 5f);
            _rewards = CreateList(_serializedQuest.FindProperty("_rewards"), "Reward", 5f);
        }

        private static ReorderableList CreateList(SerializedProperty property, string label, float lines)
        {
            var list = new ReorderableList(property.serializedObject, property, true, true, true, true);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"{label}s ({property.arraySize})");
            list.elementHeight = EditorGUIUtility.singleLineHeight * lines + 8f;
            list.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), GUIContent.none, true);
            return list;
        }

        private void SelectQuest(QuestDefinition quest) { _quest = quest; _serializedQuest = null; Repaint(); }

        private void CreateAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Quest", "NewQuestDefinition", "asset", "Choose a quest asset path.");
            if (string.IsNullOrWhiteSpace(path)) return;
            SelectQuest(CreateQuestAsset(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'), System.IO.Path.GetFileName(path)));
        }

        private static IReadOnlyList<QuestDefinition> FindQuests(string search)
        {
            return AssetDatabase.FindAssets("t:QuestDefinition").Select(guid => AssetDatabase.LoadAssetAtPath<QuestDefinition>(AssetDatabase.GUIDToAssetPath(guid))).Where(q => q != null && Matches(q, search)).OrderBy(q => q.DisplayName).ToList();
        }

        private static bool Matches(QuestDefinition quest, string search) => string.IsNullOrWhiteSpace(search) || quest.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || quest.Id.Value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || AssetDatabase.GetAssetPath(quest).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        private static void DrawLinkedAssets<T>(string label, Func<T, bool> predicate) where T : UnityEngine.Object
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(T).Name}").Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).Where(asset => asset != null && predicate(asset)).ToList();
            EditorGUILayout.LabelField($"{label}: {assets.Count}", EditorStyles.miniBoldLabel);
            foreach (var asset in assets) using (new EditorGUILayout.HorizontalScope()) { EditorGUILayout.ObjectField(asset, typeof(T), false); if (GUILayout.Button("Ping", GUILayout.Width(55f))) EditorGUIUtility.PingObject(asset); }
        }

    }
}
