using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Editor.Dashboard;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class CharacterEditorWindow : EditorWindow
    {
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private string _search = string.Empty;
        private CharacterDefinition _selected;
        private SerializedObject _serializedCharacter;

        [MenuItem("Tools/RPG Toolkit/Character Editor")]
        public static void Open()
        {
            var window = GetWindow<CharacterEditorWindow>("Characters");
            window.minSize = new Vector2(760f, 480f);
            window.Show();
        }

        public static RPGValidationResult ValidateCharacter(CharacterDefinition character, IEnumerable<CharacterDefinition> allCharacters = null)
        {
            var result = new RPGValidationResult();
            if (character == null)
            {
                result.AddError("RPG_CHARACTER_NULL", "Character definition is missing.");
                return result;
            }

            if (character.Id.IsEmpty) result.AddError("RPG_CHARACTER_MISSING_ID", "Character is missing an RPG ID.");
            var serialized = new SerializedObject(character);
            var authoredDisplayName = serialized.FindProperty("_displayName")?.stringValue;
            if (string.IsNullOrWhiteSpace(authoredDisplayName)) result.AddError("RPG_CHARACTER_MISSING_DISPLAY_NAME", "Character needs an authored display name instead of relying on the asset filename.", character.Id);
            if (character.StartingLevel < 1) result.AddError("RPG_CHARACTER_INVALID_LEVEL", "Starting level must be at least 1.", character.Id);

            var duplicateStats = new HashSet<UnityEngine.Object>();
            var seenStats = new HashSet<UnityEngine.Object>();
            foreach (var stat in character.StatTemplate)
            {
                if (stat.stat == null)
                {
                    result.AddError("RPG_CHARACTER_MISSING_STAT", "A stat template entry is missing its Stat Definition.", character.Id);
                    continue;
                }

                if (!seenStats.Add(stat.stat)) duplicateStats.Add(stat.stat);
            }

            foreach (var stat in duplicateStats) result.AddWarning("RPG_CHARACTER_DUPLICATE_STAT", $"Stat template contains duplicate stat '{stat.name}'.", character.Id);

            var seenResources = new HashSet<UnityEngine.Object>();
            foreach (var resource in character.Resources)
            {
                if (resource.resource == null)
                {
                    result.AddError("RPG_CHARACTER_MISSING_RESOURCE", "A resource entry is missing its Resource Definition.", character.Id);
                    continue;
                }

                if (!seenResources.Add(resource.resource)) result.AddWarning("RPG_CHARACTER_DUPLICATE_RESOURCE", $"Resources contain duplicate resource '{resource.resource.name}'.", character.Id);
            }

            if (allCharacters != null && !character.Id.IsEmpty)
            {
                var duplicates = allCharacters.Count(other => other != null && other != character && other.Id == character.Id);
                if (duplicates > 0) result.AddError("RPG_CHARACTER_DUPLICATE_ID", $"Character ID '{character.Id}' is used by {duplicates + 1} character assets.", character.Id);
            }

            return result;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Character Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Create, duplicate, find, validate, and edit CharacterDefinition assets without relying on the Project inspector.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            DrawCharacterList();
            DrawCharacterDetails();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCharacterList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(280f));
            _search = EditorGUILayout.TextField("Search", _search);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create")) Select(CreateCharacter());
            using (new EditorGUI.DisabledScope(_selected == null))
            {
                if (GUILayout.Button("Duplicate")) Select(DuplicateCharacter(_selected));
            }
            EditorGUILayout.EndHorizontal();

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, EditorStyles.helpBox);
            foreach (var character in FindCharacters(_search))
            {
                if (GUILayout.Toggle(_selected == character, character.DisplayName, EditorStyles.miniButton)) Select(character);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawCharacterDetails()
        {
            EditorGUILayout.BeginVertical();
            if (_selected == null)
            {
                EditorGUILayout.HelpBox("Select or create a character to edit its core data, stat template, resources, tags, and validation summary.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            if (_serializedCharacter == null || _serializedCharacter.targetObject != _selected) _serializedCharacter = new SerializedObject(_selected);
            _serializedCharacter.Update();
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Asset", _selected, typeof(CharacterDefinition), false);
            if (GUILayout.Button("Ping", GUILayout.Width(70f))) EditorGUIUtility.PingObject(_selected);
            if (GUILayout.Button("Select", GUILayout.Width(70f))) Selection.activeObject = _selected;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_id"));
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_displayName"));
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_description"));
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_startingLevel"));
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_portrait"));
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_prefab"));
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_experienceCurve"));
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_tags"), new GUIContent("Role Tags"), true);
            DrawTemplateHelpers();
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_statTemplate"), true);
            EditorGUILayout.PropertyField(_serializedCharacter.FindProperty("_resources"), true);
            _serializedCharacter.ApplyModifiedProperties();
            DrawValidationSummary();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTemplateHelpers()
        {
            EditorGUILayout.LabelField("Stat Template Helpers", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use role tags such as party, enemy, companion, boss, or vendor to classify characters for game-specific systems. Add Stat Definition and Resource Definition entries below to build the runtime template.", MessageType.Info);
        }

        private void DrawValidationSummary()
        {
            var result = ValidateCharacter(_selected, FindCharacters(null));
            EditorGUILayout.LabelField("Validation Summary", EditorStyles.boldLabel);
            if (result.Messages.Count == 0)
            {
                EditorGUILayout.HelpBox("Character is valid.", MessageType.Info);
                return;
            }

            foreach (var message in result.Messages)
            {
                EditorGUILayout.HelpBox($"[{message.Code}] {message.Message}", message.IsError ? MessageType.Error : MessageType.Warning);
            }
        }

        private static IReadOnlyList<CharacterDefinition> FindCharacters(string search)
        {
            return RPGToolkitAuthoringWorkflow.FindAssets(RPGToolkitAuthoringWorkflow.Sections.First(section => section.AssetType == typeof(CharacterDefinition)), search)
                .Select(entry => entry.Asset as CharacterDefinition)
                .Where(asset => asset != null)
                .OrderBy(asset => asset.DisplayName)
                .ToList();
        }

        private static CharacterDefinition CreateCharacter()
        {
            var section = RPGToolkitAuthoringWorkflow.Sections.First(s => s.AssetType == typeof(CharacterDefinition));
            return RPGToolkitAuthoringWorkflow.CreateAsset(section) as CharacterDefinition;
        }

        private static CharacterDefinition DuplicateCharacter(CharacterDefinition source)
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

        private void Select(CharacterDefinition character)
        {
            _selected = character;
            _serializedCharacter = character == null ? null : new SerializedObject(character);
            Selection.activeObject = character;
        }

    }
}
