using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SixStringSyn.RPGToolkit2D.Editor.Dashboard;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class ItemDatabaseWindow : EditorWindow
    {
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private string _search = string.Empty;
        private ItemTypeFilter _typeFilter = ItemTypeFilter.All;
        private ItemRarityFilter _rarityFilter = ItemRarityFilter.All;
        private StackabilityFilter _stackabilityFilter = StackabilityFilter.All;
        private ItemDefinition _selected;
        private SerializedObject _serializedItem;

        private enum ItemTypeFilter { All, Generic, Consumable, Weapon, Armor, QuestItem }
        private enum ItemRarityFilter { All, Common, Uncommon, Rare, Epic, Legendary }
        public enum StackabilityFilter { All, Stackable, NotStackable }

        [MenuItem("Tools/RPG Toolkit/Item Database")]
        public static void Open()
        {
            var window = GetWindow<ItemDatabaseWindow>("RPG Items");
            window.minSize = new Vector2(820f, 500f);
            window.Show();
        }

        public static IReadOnlyList<ItemDefinition> FilterItems(IEnumerable<ItemDefinition> items, string search, ItemType? itemType, ItemRarity? rarity, StackabilityFilter stackability)
        {
            if (items == null) return Array.Empty<ItemDefinition>();
            return items.Where(item => item != null)
                .Where(item => MatchesSearch(item, search))
                .Where(item => !itemType.HasValue || item.ItemType == itemType.Value)
                .Where(item => !rarity.HasValue || item.Rarity == rarity.Value)
                .Where(item => stackability == StackabilityFilter.All || (stackability == StackabilityFilter.Stackable && item.IsStackable) || (stackability == StackabilityFilter.NotStackable && !item.IsStackable))
                .OrderBy(item => item.ItemType)
                .ThenBy(item => item.Rarity)
                .ThenBy(item => item.DisplayName)
                .ToList();
        }

        public static RPGValidationResult ValidateItem(ItemDefinition item, IEnumerable<ItemDefinition> allItems = null)
        {
            var result = new RPGValidationResult();
            if (item == null)
            {
                result.AddError("RPG_ITEM_NULL", "Item definition is missing.");
                return result;
            }

            if (item.Id.IsEmpty) result.AddError("RPG_ITEM_MISSING_ID", "Item is missing an RPG ID.");
            var serialized = new SerializedObject(item);
            var authoredDisplayName = serialized.FindProperty("_displayName")?.stringValue;
            if (string.IsNullOrWhiteSpace(authoredDisplayName)) result.AddError("RPG_ITEM_MISSING_DISPLAY_NAME", "Item needs an authored display name instead of relying on the asset filename.", item.Id);
            var authoredStackSize = serialized.FindProperty("_maximumStackSize")?.intValue ?? item.MaximumStackSize;
            if (authoredStackSize < 1) result.AddError("RPG_ITEM_INVALID_STACK_SIZE", "Maximum stack size must be at least 1.", item.Id);

            var useActionProperty = serialized.FindProperty("_useAction");
            var useAction = useActionProperty?.objectReferenceValue;
            if (useActionProperty != null && useAction == null && useActionProperty.objectReferenceInstanceIDValue != 0) result.AddError("RPG_ITEM_BROKEN_USE_ACTION", "Item use action reference is broken or missing from the project.", item.Id);
            if (item.ItemType == ItemType.Consumable && useAction == null) result.AddWarning("RPG_ITEM_CONSUMABLE_NO_USE_ACTION", "Consumable item has no use action assigned.", item.Id);
            if ((item.ItemType == ItemType.Weapon || item.ItemType == ItemType.Armor) && item.AllowedEquipmentSlots.Count == 0) result.AddWarning("RPG_ITEM_EQUIPMENT_NO_SLOTS", "Equipment item has no allowed equipment slots.", item.Id);

            if (allItems != null && !item.Id.IsEmpty)
            {
                var duplicates = allItems.Count(other => other != null && other != item && other.Id == item.Id);
                if (duplicates > 0) result.AddError("RPG_ITEM_DUPLICATE_ID", $"Item ID '{item.Id}' is used by {duplicates + 1} item assets.", item.Id);
            }

            return result;
        }

        public static int RepairDuplicateItemIds(IEnumerable<ItemDefinition> items)
        {
            var repaired = 0;
            var seen = new HashSet<RPGId>();
            foreach (var item in (items ?? Array.Empty<ItemDefinition>()).Where(item => item != null))
            {
                if (item.Id.IsEmpty || seen.Add(item.Id)) continue;
                Undo.RecordObject(item, "Repair Duplicate Item ID");
                item.AssignNewId();
                EditorUtility.SetDirty(item);
                repaired++;
            }

            if (repaired > 0) AssetDatabase.SaveAssets();
            return repaired;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Item Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Browse, filter, create, duplicate, validate, repair, export, and edit ItemDefinition assets from one focused workflow.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            DrawItemList();
            DrawItemDetails();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawItemList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(330f));
            _search = EditorGUILayout.TextField("Search", _search);
            _typeFilter = (ItemTypeFilter)EditorGUILayout.EnumPopup("Type", _typeFilter);
            _rarityFilter = (ItemRarityFilter)EditorGUILayout.EnumPopup("Rarity", _rarityFilter);
            _stackabilityFilter = (StackabilityFilter)EditorGUILayout.EnumPopup("Stackability", _stackabilityFilter);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create")) Select(CreateItem());
            using (new EditorGUI.DisabledScope(_selected == null))
            {
                if (GUILayout.Button("Duplicate")) Select(DuplicateItem(_selected));
                if (GUILayout.Button("Delete") && EditorUtility.DisplayDialog("Delete Item", $"Delete {_selected.DisplayName}? This cannot be undone.", "Delete", "Cancel")) DeleteSelectedItem();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Repair Duplicate IDs")) EditorUtility.DisplayDialog("Item ID Repair", $"Updated {RepairDuplicateItemIds(FindItems(null))} duplicate item ID(s).", "OK");
            if (GUILayout.Button("Export CSV")) ExportCsv(FindItems(null));
            EditorGUILayout.EndHorizontal();

            var items = FindItems(_search, GetTypeFilter(), GetRarityFilter(), _stackabilityFilter);
            DrawCounts(items);
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, EditorStyles.helpBox);
            foreach (var item in items)
            {
                var label = $"{item.DisplayName}  [{item.ItemType} / {item.Rarity} / Stack {item.MaximumStackSize}]";
                if (GUILayout.Toggle(_selected == item, label, EditorStyles.miniButton)) Select(item);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawItemDetails()
        {
            EditorGUILayout.BeginVertical();
            if (_selected == null)
            {
                EditorGUILayout.HelpBox("Select or create an item to edit IDs, display metadata, icons, stack size, item type, rarity, equipment slots, use actions, tags, and validation issues.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            if (_serializedItem == null || _serializedItem.targetObject != _selected) _serializedItem = new SerializedObject(_selected);
            _serializedItem.Update();
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Asset", _selected, typeof(ItemDefinition), false);
            if (GUILayout.Button("Ping", GUILayout.Width(70f))) EditorGUIUtility.PingObject(_selected);
            if (GUILayout.Button("Select", GUILayout.Width(70f))) Selection.activeObject = _selected;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_id"));
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_displayName"));
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_description"));
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_tags"), true);
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_icon"));
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_maximumStackSize"));
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_rarity"));
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_itemType"));
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_allowedEquipmentSlots"), true);
            EditorGUILayout.PropertyField(_serializedItem.FindProperty("_useAction"));
            _serializedItem.ApplyModifiedProperties();
            DrawValidationSummary();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawCounts(IReadOnlyList<ItemDefinition> items)
        {
            EditorGUILayout.LabelField($"{items.Count} item(s) found", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("By type: " + string.Join(", ", items.GroupBy(item => item.ItemType).Select(group => $"{group.Key} {group.Count()}")), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("By rarity: " + string.Join(", ", items.GroupBy(item => item.Rarity).Select(group => $"{group.Key} {group.Count()}")), EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawValidationSummary()
        {
            var result = ValidateItem(_selected, FindItems(null));
            EditorGUILayout.LabelField("Validation Summary", EditorStyles.boldLabel);
            if (result.Messages.Count == 0) EditorGUILayout.HelpBox("Item is valid.", MessageType.Info);
            foreach (var message in result.Messages) EditorGUILayout.HelpBox($"[{message.Code}] {message.Message}", message.IsError ? MessageType.Error : MessageType.Warning);
        }

        private static IReadOnlyList<ItemDefinition> FindItems(string search, ItemType? itemType = null, ItemRarity? rarity = null, StackabilityFilter stackability = StackabilityFilter.All)
        {
            var section = RPGToolkitAuthoringWorkflow.Sections.First(s => s.AssetType == typeof(ItemDefinition));
            var items = RPGToolkitAuthoringWorkflow.FindAssets(section).Select(entry => entry.Asset as ItemDefinition);
            return FilterItems(items, search, itemType, rarity, stackability);
        }

        private static bool MatchesSearch(ItemDefinition item, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return true;
            var path = AssetDatabase.GetAssetPath(item) ?? string.Empty;
            return item.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || item.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || item.Id.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static ItemDefinition CreateItem() => RPGToolkitAuthoringWorkflow.CreateAsset(RPGToolkitAuthoringWorkflow.Sections.First(s => s.AssetType == typeof(ItemDefinition))) as ItemDefinition;

        private static ItemDefinition DuplicateItem(ItemDefinition source)
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

        private void DeleteSelectedItem()
        {
            var path = AssetDatabase.GetAssetPath(_selected);
            Select(null);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
        }

        private void Select(ItemDefinition item)
        {
            _selected = item;
            _serializedItem = item == null ? null : new SerializedObject(item);
            Selection.activeObject = item;
        }

        private ItemType? GetTypeFilter() => _typeFilter == ItemTypeFilter.All ? (ItemType?)null : (ItemType)(_typeFilter - 1);
        private ItemRarity? GetRarityFilter() => _rarityFilter == ItemRarityFilter.All ? (ItemRarity?)null : (ItemRarity)(_rarityFilter - 1);

        private static void ExportCsv(IEnumerable<ItemDefinition> items)
        {
            var path = EditorUtility.SaveFilePanel("Export Item Database CSV", Application.dataPath, "item-database.csv", "csv");
            if (string.IsNullOrWhiteSpace(path)) return;
            var builder = new StringBuilder("Id,Display Name,Type,Rarity,Stackable,Maximum Stack Size,Asset Path\n");
            foreach (var item in items.Where(item => item != null)) builder.AppendLine($"{Escape(item.Id.ToString())},{Escape(item.DisplayName)},{item.ItemType},{item.Rarity},{item.IsStackable},{item.MaximumStackSize},{Escape(AssetDatabase.GetAssetPath(item))}");
            File.WriteAllText(path, builder.ToString());
        }

        private static string Escape(string value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    }
}
