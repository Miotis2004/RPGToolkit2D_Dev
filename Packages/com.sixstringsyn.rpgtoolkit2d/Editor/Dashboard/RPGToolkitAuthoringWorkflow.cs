using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Abilities;
using SixStringSyn.RPGToolkit2D.Runtime.Characters;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Loot;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using SixStringSyn.RPGToolkit2D.Runtime.NPCs;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using SixStringSyn.RPGToolkit2D.Runtime.Vendors;
using SixStringSyn.RPGToolkit2D.Editor;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Dashboard
{
    public enum RPGToolkitDashboardCapabilityStatus
    {
        Complete,
        Partial,
        Missing,
        NotApplicable
    }

    public sealed class RPGToolkitDashboardCapability
    {
        public RPGToolkitDashboardCapability(
            string contentTitle,
            Type assetType,
            RPGToolkitDashboardCapabilityStatus assetCreationStatus,
            RPGToolkitDashboardCapabilityStatus focusedEditorStatus,
            RPGToolkitDashboardCapabilityStatus validationStatus,
            RPGToolkitDashboardCapabilityStatus documentationStatus,
            RPGToolkitDashboardCapabilityStatus runtimeIntegrationStatus,
            string notes)
        {
            ContentTitle = contentTitle;
            AssetType = assetType;
            AssetCreationStatus = assetCreationStatus;
            FocusedEditorStatus = focusedEditorStatus;
            ValidationStatus = validationStatus;
            DocumentationStatus = documentationStatus;
            RuntimeIntegrationStatus = runtimeIntegrationStatus;
            Notes = notes;
        }

        public string ContentTitle { get; }
        public Type AssetType { get; }
        public RPGToolkitDashboardCapabilityStatus AssetCreationStatus { get; }
        public RPGToolkitDashboardCapabilityStatus FocusedEditorStatus { get; }
        public RPGToolkitDashboardCapabilityStatus ValidationStatus { get; }
        public RPGToolkitDashboardCapabilityStatus DocumentationStatus { get; }
        public RPGToolkitDashboardCapabilityStatus RuntimeIntegrationStatus { get; }
        public string Notes { get; }
    }

    public sealed class RPGToolkitAuthoringSection
    {
        public RPGToolkitAuthoringSection(string title, string description, Type assetType, string defaultFileName, string menuPath, string documentationPath, string setupHint, bool warnWhenEmpty, RPGToolkitDashboardCapability capability, Action openEditor = null)
        {
            Title = title;
            Description = description;
            AssetType = assetType;
            DefaultFileName = defaultFileName;
            MenuPath = menuPath;
            DocumentationPath = documentationPath;
            SetupHint = setupHint ?? string.Empty;
            WarnWhenEmpty = warnWhenEmpty;
            Capability = capability ?? throw new ArgumentNullException(nameof(capability));
            OpenEditor = openEditor;
        }

        public string Title { get; }
        public string Description { get; }
        public Type AssetType { get; }
        public string DefaultFileName { get; }
        public string MenuPath { get; }
        public string DocumentationPath { get; }
        public string SetupHint { get; }
        public bool WarnWhenEmpty { get; }
        public RPGToolkitDashboardCapability Capability { get; }
        public Action OpenEditor { get; }
    }

    public sealed class RPGToolkitDashboardCardData
    {
        public RPGToolkitDashboardCardData(RPGToolkitAuthoringSection section, IReadOnlyList<RPGToolkitAssetBrowserEntry> entries, int invalidAssetCount, int validationMessageCount, string lastValidationSummary, int questMissingObjectiveCount = 0, int questMissingRewardCount = 0)
        {
            Section = section ?? throw new ArgumentNullException(nameof(section));
            Entries = entries ?? Array.Empty<RPGToolkitAssetBrowserEntry>();
            AssetCount = Entries.Count;
            DuplicateIdCount = Entries.Count(entry => entry.DuplicateId);
            InvalidAssetCount = invalidAssetCount;
            ValidationMessageCount = validationMessageCount;
            LastValidationSummary = lastValidationSummary ?? "Not validated yet.";
            QuestMissingObjectiveCount = questMissingObjectiveCount;
            QuestMissingRewardCount = questMissingRewardCount;
        }

        public RPGToolkitAuthoringSection Section { get; }
        public IReadOnlyList<RPGToolkitAssetBrowserEntry> Entries { get; }
        public int AssetCount { get; }
        public int DuplicateIdCount { get; }
        public int InvalidAssetCount { get; }
        public int ValidationMessageCount { get; }
        public string LastValidationSummary { get; }
        public int QuestMissingObjectiveCount { get; }
        public int QuestMissingRewardCount { get; }
        public bool HasWarnings => DuplicateIdCount > 0 || InvalidAssetCount > 0 || QuestMissingObjectiveCount > 0 || QuestMissingRewardCount > 0 || (Section.WarnWhenEmpty && AssetCount == 0);
        public string EmptyContentWarning => Section.WarnWhenEmpty && AssetCount == 0 ? $"No {Section.Title.ToLowerInvariant()} found yet." : string.Empty;
    }

    public sealed class RPGToolkitAssetBrowserEntry
    {
        public RPGToolkitAssetBrowserEntry(UnityEngine.Object asset, string path, bool duplicateId)
        {
            Asset = asset;
            Path = path;
            DuplicateId = duplicateId;
        }

        public UnityEngine.Object Asset { get; }
        public string Path { get; }
        public bool DuplicateId { get; }
    }

    public static class RPGToolkitAuthoringWorkflow
    {
        public const string DefaultAssetFolder = "Assets/RPGToolkit2D";
        public const string EditorToolsDocumentationPath = RPGToolkitPackageValidator.PackagePath + "/Documentation~/editor-tools.md";
        private const string EditorToolsDocumentationAnchorPrefix = EditorToolsDocumentationPath + "#";

        private static readonly IReadOnlyList<RPGToolkitAuthoringSection> _sections = new List<RPGToolkitAuthoringSection>
        {
            CreateSection("Characters", "Create heroes, enemies, party members, and stat templates.", typeof(CharacterDefinition), "NewCharacterDefinition.asset", "Tools/RPG Toolkit/Character Editor", EditorToolsDocumentationAnchorPrefix + "characters", "Characters usually need stats, resources, tags, and save/party integration before use.", true, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Character Editor supports creation, duplication, focused field editing, stat/resource helpers, role tags, search, and validation.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.CharacterEditorWindow.Open),
            CreateSection("Items", "Create consumables, equipment, quest items, and inventory content.", typeof(ItemDefinition), "NewItemDefinition.asset", "Assets/Create/RPG Toolkit/Item Definition", EditorToolsDocumentationAnchorPrefix + "items", "Items power inventory, vendors, loot, crafting, quests, and pickups.", true, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Item Editor supports search, filters, creation, duplication, deletion, focused editing, validation, duplicate ID repair, and CSV export.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.ItemDatabaseWindow.Open),
            CreateSection("Quests", "Author objectives, conditions, rewards, and turn-in behavior.", typeof(QuestDefinition), "NewQuestDefinition.asset", "Tools/RPG Toolkit/Quest Editor", EditorToolsDocumentationAnchorPrefix + "quests", "Quests need at least one objective and often reference items, NPCs, dialogue, or world-state keys.", false, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Quest Editor supports search, creation, metadata, reorderable objectives/conditions/rewards, quick links, validation, dependency views, and safe repairs.", global::SixStringSyn.RPGToolkit2D.Editor.QuestEditor.QuestEditorWindow.Open),
            CreateSection("Dialogue", "Build branching conversations and command-driven narrative flow.", typeof(DialogueDefinition), "NewDialogueDefinition.asset", "Tools/RPG Toolkit/Dialogue Graph Editor", EditorToolsDocumentationAnchorPrefix + "dialogue", "Dialogue is most useful when linked from NPCs and backed by conditions or commands.", false, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Dialogue Graph is available but still needs complete visual workflow coverage while section-specific docs are available.", global::SixStringSyn.RPGToolkit2D.Editor.DialogueGraph.DialogueGraphEditorWindow.Open),
            CreateSection("Abilities", "Create reusable abilities for combat, interactions, and custom effects.", typeof(AbilityDefinition), "NewAbilityDefinition.asset", "Tools/RPG Toolkit/Ability Editor", EditorToolsDocumentationAnchorPrefix + "abilities", "Abilities should define targeting, costs, cooldowns, tags, and runtime effects.", false, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Ability Editor supports creation, duplication, focused metadata/targeting/cost/effect editing, and validation.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.AbilityEditorWindow.Open),
            CreateSection("Vendors", "Configure shops, prices, stock, buy/sell rules, and inventory integration.", typeof(VendorDefinition), "NewVendorDefinition.asset", "Tools/RPG Toolkit/Vendor Editor", EditorToolsDocumentationAnchorPrefix + "vendors", "Vendors need item stock and pricing rules before they are useful in-game.", false, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Vendor Editor supports creation, duplication, focused stock/pricing editing, Item Database navigation, and validation.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.VendorEditorWindow.Open),
            CreateSection("Loot Tables", "Define weighted drops for chests, encounters, rewards, and vendors.", typeof(LootTableDefinition), "NewLootTableDefinition.asset", "Tools/RPG Toolkit/Loot Table Editor", EditorToolsDocumentationAnchorPrefix + "loot-tables", "Loot tables need at least one weighted item or nested-table entry.", false, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Loot Table Editor supports creation, duplication, weighted-entry editing, validation, and simulation previews.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.LootTableEditorWindow.Open),
            CreateSection("NPCs", "Create NPC metadata, dialogue links, party hooks, and world-state keys.", typeof(NPCDefinition), "NewNPCDefinition.asset", "Tools/RPG Toolkit/NPC Editor", EditorToolsDocumentationAnchorPrefix + "npcs", "NPCs often need dialogue links, world-state keys, schedules, or quest relationships.", false, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "NPC Editor supports creation, duplication, focused link editing, relationship discovery, quick actions, and validation.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.NPCEditorWindow.Open),
            CreateSection("Maps", "Author tile layers, zones, entrances, exits, object placements, and transitions.", typeof(RPGMapDefinition), "NewMapDefinition.asset", "Tools/RPG Toolkit/Maps/Map Editor", EditorToolsDocumentationAnchorPrefix + "maps", "Maps need a tileset before tile layers can be painted reliably.", true, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Map authoring has focused tools and validation entry points; deep-link documentation is available.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.MapEditorWindow.Open),
            CreateSection("Tilesets", "Bridge sprite frames to tile metadata, collision defaults, palettes, and map painting.", typeof(RPGTilesetDefinition), "NewTilesetDefinition.asset", "Tools/RPG Toolkit/Maps/Tileset Editor", EditorToolsDocumentationAnchorPrefix + "tilesets", "Tilesets need sprite sheet/frame metadata before maps can reference tiles safely.", false, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Tileset authoring has focused tools and validation entry points; deep-link documentation is available.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.TilesetEditorWindow.Open),
            CreateSection("Sprite Sheets", "Manage source textures, generated frame metadata, tags, groups, and tile defaults.", typeof(RPGSpriteSheetAsset), "NewSpriteSheetAsset.asset", "Tools/RPG Toolkit/Maps/Sprite Sheet Editor", EditorToolsDocumentationAnchorPrefix + "sprite-sheets", "Sprite sheets need frame metadata, tags, groups, and tile defaults for tilesets.", false, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Sprite sheet authoring has focused tools and validation entry points; deep-link documentation is available.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.SpriteSheetEditorWindow.Open),
            CreateSection("Sprite Sheet Profiles", "Define slicing rules, grid dimensions, naming patterns, pivots, and pixels per unit.", typeof(RPGSpriteSheetProfile), "NewSpriteSheetProfile.asset", "Assets/Create/RPG Toolkit/Foundation/Sprite Sheet Profile", EditorToolsDocumentationAnchorPrefix + "sprite-sheet-profiles", "Profiles define repeatable slicing settings used before generating sprite sheet frame data.", false, RPGToolkitDashboardCapabilityStatus.Missing, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Profiles support asset creation/runtime use, but no focused profile editor exists yet; deep-link documentation is available.")
        };

        public static IReadOnlyList<RPGToolkitAuthoringSection> Sections => _sections;

        public static IReadOnlyList<RPGToolkitDashboardCapability> Capabilities => _sections.Select(section => section.Capability).ToList();

        private static RPGToolkitAuthoringSection CreateSection(string title, string description, Type assetType, string defaultFileName, string menuPath, string documentationPath, string setupHint, bool warnWhenEmpty, RPGToolkitDashboardCapabilityStatus focusedEditorStatus, RPGToolkitDashboardCapabilityStatus validationStatus, RPGToolkitDashboardCapabilityStatus documentationStatus, RPGToolkitDashboardCapabilityStatus runtimeIntegrationStatus, string notes, Action openEditor = null)
        {
            var capability = new RPGToolkitDashboardCapability(title, assetType, RPGToolkitDashboardCapabilityStatus.Complete, focusedEditorStatus, validationStatus, documentationStatus, runtimeIntegrationStatus, notes);
            return new RPGToolkitAuthoringSection(title, description, assetType, defaultFileName, menuPath, documentationPath, setupHint, warnWhenEmpty, capability, openEditor);
        }

        [MenuItem("Tools/RPG Toolkit/Maps/Create Map Definition")]
        public static void CreateMapDefinition() => CreateAsset(Sections.First(section => section.AssetType == typeof(RPGMapDefinition)));

        [MenuItem("Tools/RPG Toolkit/Maps/Create Tileset Definition")]
        public static void CreateTilesetDefinition() => CreateAsset(Sections.First(section => section.AssetType == typeof(RPGTilesetDefinition)));

        [MenuItem("Tools/RPG Toolkit/Maps/Create Sprite Sheet Asset")]
        public static void CreateSpriteSheetAsset() => CreateAsset(Sections.First(section => section.AssetType == typeof(RPGSpriteSheetAsset)));

        [MenuItem("Tools/RPG Toolkit/Maps/Create Sprite Sheet Profile")]
        public static void CreateSpriteSheetProfile() => CreateAsset(Sections.First(section => section.AssetType == typeof(RPGSpriteSheetProfile)));

        public static UnityEngine.Object CreateAsset(RPGToolkitAuthoringSection section, string folder = DefaultAssetFolder)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            if (!typeof(ScriptableObject).IsAssignableFrom(section.AssetType)) throw new ArgumentException($"{section.AssetType.Name} is not a ScriptableObject.", nameof(section));

            EnsureFolder(folder);
            var asset = ScriptableObject.CreateInstance(section.AssetType);
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, section.DefaultFileName).Replace('\\', '/'));
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            return asset;
        }

        public static IReadOnlyList<RPGToolkitAssetBrowserEntry> FindAssets(RPGToolkitAuthoringSection section, string search = null)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            var entries = new List<RPGToolkitAssetBrowserEntry>();
            var ids = new Dictionary<RPGId, int>();
            var guids = AssetDatabase.FindAssets($"t:{section.AssetType.Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, section.AssetType);
                if (asset == null) continue;
                if (!MatchesSearch(asset, path, search)) continue;
                if (asset is RPGObject rpgAsset && !rpgAsset.Id.IsEmpty) ids[rpgAsset.Id] = ids.TryGetValue(rpgAsset.Id, out var count) ? count + 1 : 1;
                entries.Add(new RPGToolkitAssetBrowserEntry(asset, path, false));
            }

            return entries.Select(entry => new RPGToolkitAssetBrowserEntry(entry.Asset, entry.Path, entry.Asset is RPGObject rpgAsset && !rpgAsset.Id.IsEmpty && ids.TryGetValue(rpgAsset.Id, out var count) && count > 1)).ToList();
        }

        public static RPGToolkitDashboardCardData BuildCardData(RPGToolkitAuthoringSection section, IReadOnlyDictionary<string, string> lastValidationResults = null)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            var entries = FindAssets(section);
            var invalidAssetCount = 0;
            var validationMessageCount = 0;
            var questMissingObjectiveCount = 0;
            var questMissingRewardCount = 0;

            foreach (var entry in entries)
            {
                var result = ValidateAsset(entry.Asset);
                if (entry.Asset is QuestDefinition quest)
                {
                    if (quest.Objectives.Count == 0) questMissingObjectiveCount++;
                    if (quest.Rewards.Count == 0) questMissingRewardCount++;
                }
                if (result == null) continue;
                validationMessageCount += result.Messages.Count;
                if (!result.IsValid) invalidAssetCount++;
            }

            var summary = lastValidationResults != null && lastValidationResults.TryGetValue(section.Title, out var lastResult) ? lastResult : "Not validated yet.";
            return new RPGToolkitDashboardCardData(section, entries, invalidAssetCount, validationMessageCount, summary, questMissingObjectiveCount, questMissingRewardCount);
        }

        public static RPGToolkitDashboardCardData ValidateCard(RPGToolkitAuthoringSection section, IDictionary<string, string> lastValidationResults)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            if (lastValidationResults == null) throw new ArgumentNullException(nameof(lastValidationResults));
            var data = BuildCardData(section, lastValidationResults);
            lastValidationResults[section.Title] = $"{DateTime.Now:HH:mm:ss}: {data.AssetCount} asset(s), {data.DuplicateIdCount} duplicate ID asset(s), {data.InvalidAssetCount} invalid asset(s), {data.ValidationMessageCount} validation message(s).";
            return BuildCardData(section, lastValidationResults);
        }

        private static RPGValidationResult ValidateAsset(UnityEngine.Object asset)
        {
            switch (asset)
            {
                case QuestDefinition quest: return quest.ValidateQuest();
                case DialogueDefinition dialogue: return dialogue.ValidateGraph();
                case AbilityDefinition ability: return global::SixStringSyn.RPGToolkit2D.Editor.Windows.AbilityEditorWindow.ValidateAbility(ability, FindAssets(Sections.First(section => section.AssetType == typeof(AbilityDefinition))).Select(entry => entry.Asset as AbilityDefinition).Where(asset => asset != null));
                case VendorDefinition vendor: return global::SixStringSyn.RPGToolkit2D.Editor.Windows.VendorEditorWindow.ValidateVendor(vendor, FindAssets(Sections.First(section => section.AssetType == typeof(VendorDefinition))).Select(entry => entry.Asset as VendorDefinition).Where(asset => asset != null));
                case LootTableDefinition lootTable: return global::SixStringSyn.RPGToolkit2D.Editor.Windows.LootTableEditorWindow.ValidateLootTable(lootTable, FindAssets(Sections.First(section => section.AssetType == typeof(LootTableDefinition))).Select(entry => entry.Asset as LootTableDefinition).Where(asset => asset != null));
                case RPGMapDefinition map: return map.ValidateMap();
                case RPGTilesetDefinition tileset: return tileset.ValidateTileset();
                case RPGSpriteSheetAsset spriteSheet: return spriteSheet.ValidateSpriteSheet();
                case CharacterDefinition character: return global::SixStringSyn.RPGToolkit2D.Editor.Windows.CharacterEditorWindow.ValidateCharacter(character, FindAssets(Sections.First(section => section.AssetType == typeof(CharacterDefinition))).Select(entry => entry.Asset as CharacterDefinition).Where(asset => asset != null));
                case ItemDefinition item: return global::SixStringSyn.RPGToolkit2D.Editor.Windows.ItemDatabaseWindow.ValidateItem(item, FindAssets(Sections.First(section => section.AssetType == typeof(ItemDefinition))).Select(entry => entry.Asset as ItemDefinition).Where(asset => asset != null));
                case NPCDefinition npc: return global::SixStringSyn.RPGToolkit2D.Editor.Windows.NPCEditorWindow.ValidateNpc(npc, FindAssets(Sections.First(section => section.AssetType == typeof(NPCDefinition))).Select(entry => entry.Asset as NPCDefinition).Where(asset => asset != null));
                default: return null;
            }
        }


        public static bool HasFocusedTool(RPGToolkitAuthoringSection section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            return section.OpenEditor != null || !string.IsNullOrWhiteSpace(section.MenuPath);
        }

        public static bool TryOpenFocusedTool(RPGToolkitAuthoringSection section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            if (section.Capability.FocusedEditorStatus == RPGToolkitDashboardCapabilityStatus.Missing)
            {
                Debug.LogWarning($"RPG Toolkit dashboard: no focused tool is available for {section.Title}. Opening documentation instead.");
                OpenDocumentation(section.DocumentationPath);
                return false;
            }

            if (section.OpenEditor != null)
            {
                section.OpenEditor.Invoke();
                return true;
            }

            if (!string.IsNullOrWhiteSpace(section.MenuPath) && EditorApplication.ExecuteMenuItem(section.MenuPath)) return true;

            Debug.LogWarning($"RPG Toolkit dashboard: no focused tool is available for {section.Title}. Opening documentation instead.");
            OpenDocumentation(section.DocumentationPath);
            return false;
        }

        public static bool DocumentationExists(RPGToolkitAuthoringSection section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            return File.Exists(GetDocumentationFilePath(section.DocumentationPath));
        }

        public static bool TryOpenDocumentation(RPGToolkitAuthoringSection section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            if (!DocumentationExists(section))
            {
                Debug.LogWarning($"RPG Toolkit dashboard: documentation for {section.Title} was not found at {section.DocumentationPath}.");
                return false;
            }

            OpenDocumentation(section.DocumentationPath);
            return true;
        }

        private static string GetDocumentationFilePath(string path)
        {
            var anchorIndex = path.IndexOf('#');
            return anchorIndex >= 0 ? path.Substring(0, anchorIndex) : path;
        }

        public static IReadOnlyList<PackageValidationResult> ValidateProjectSetup()
        {
            var results = new List<PackageValidationResult>(RPGToolkitPackageValidator.ValidatePackageFoundation())
            {
                ValidateManifestDependency("com.unity.inputsystem", true),
                ValidateManifestDependency("com.unity.test-framework", true),
                ValidateManifestDependency("com.unity.cinemachine", false),
                ValidateManifestDependency("com.unity.addressables", false),
                ValidateManifestDependency("com.unity.ai.navigation", false),
                new PackageValidationResult("Authoring asset folder", Directory.Exists(DefaultAssetFolder), Directory.Exists(DefaultAssetFolder) ? $"Found {DefaultAssetFolder}." : $"Create {DefaultAssetFolder} from the dashboard before adding content.")
            };
            return results;
        }

        public static void OpenDocumentation(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            var anchorIndex = path.IndexOf('#');
            if (anchorIndex < 0)
            {
                EditorUtility.OpenWithDefaultApp(path);
                return;
            }

            var filePath = path.Substring(0, anchorIndex);
            var anchor = path.Substring(anchorIndex);
            var absolutePath = Path.GetFullPath(filePath);
            Application.OpenURL(new Uri(absolutePath).AbsoluteUri + anchor);
        }

        private static bool MatchesSearch(UnityEngine.Object asset, string path, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return true;
            var displayName = asset is RPGObject rpgAsset ? rpgAsset.DisplayName : asset.name;
            var id = asset is RPGObject objectWithId ? objectWithId.Id.ToString() : string.Empty;
            return asset.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || displayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || id.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static PackageValidationResult ValidateManifestDependency(string dependency, bool required)
        {
            var manifestPath = "Packages/manifest.json";
            var found = File.Exists(manifestPath) && File.ReadAllText(manifestPath).Contains($"\"{dependency}\"");
            var label = required ? $"Required package {dependency}" : $"Recommended package {dependency}";
            var message = found ? "Installed in Packages/manifest.json." : required ? "Missing from Packages/manifest.json." : "Not installed; add it when using related samples or integrations.";
            return new PackageValidationResult(label, found || !required, message);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parts = folder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
