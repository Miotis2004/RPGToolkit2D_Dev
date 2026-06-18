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
        public RPGToolkitAuthoringSection(string title, string description, Type assetType, string defaultFileName, string menuPath, string documentationPath, RPGToolkitDashboardCapability capability, Action openEditor = null)
        {
            Title = title;
            Description = description;
            AssetType = assetType;
            DefaultFileName = defaultFileName;
            MenuPath = menuPath;
            DocumentationPath = documentationPath;
            Capability = capability ?? throw new ArgumentNullException(nameof(capability));
            OpenEditor = openEditor;
        }

        public string Title { get; }
        public string Description { get; }
        public Type AssetType { get; }
        public string DefaultFileName { get; }
        public string MenuPath { get; }
        public string DocumentationPath { get; }
        public RPGToolkitDashboardCapability Capability { get; }
        public Action OpenEditor { get; }
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
            CreateSection("Characters", "Create heroes, enemies, party members, and stat templates.", typeof(CharacterDefinition), "NewCharacterDefinition.asset", "Assets/Create/RPG Toolkit/Character Definition", EditorToolsDocumentationAnchorPrefix + "characters", RPGToolkitDashboardCapabilityStatus.Missing, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Asset creation and database discovery work; a focused Character Editor; per-card documentation links are available."),
            CreateSection("Items", "Create consumables, equipment, quest items, and inventory content.", typeof(ItemDefinition), "NewItemDefinition.asset", "Assets/Create/RPG Toolkit/Item Definition", EditorToolsDocumentationAnchorPrefix + "items", RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Item assets are supported and the Item Database exists, but full editing and validation are partial while deep-link docs are available.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.ItemDatabaseWindow.Open),
            CreateSection("Quests", "Author objectives, conditions, rewards, and turn-in behavior.", typeof(QuestDefinition), "NewQuestDefinition.asset", "Tools/RPG Toolkit/Quest Editor", EditorToolsDocumentationAnchorPrefix + "quests", RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Quest Editor is available for core workflows, but it is not yet a full visual workflow editor.", global::SixStringSyn.RPGToolkit2D.Editor.QuestEditor.QuestEditorWindow.Open),
            CreateSection("Dialogue", "Build branching conversations and command-driven narrative flow.", typeof(DialogueDefinition), "NewDialogueDefinition.asset", "Tools/RPG Toolkit/Dialogue Graph Editor", EditorToolsDocumentationAnchorPrefix + "dialogue", RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Dialogue Graph is available but still needs complete visual workflow coverage while section-specific docs are available.", global::SixStringSyn.RPGToolkit2D.Editor.DialogueGraph.DialogueGraphEditorWindow.Open),
            CreateSection("Abilities", "Create reusable abilities for combat, interactions, and custom effects.", typeof(AbilityDefinition), "NewAbilityDefinition.asset", "Tools/RPG Toolkit/Ability Editor", EditorToolsDocumentationAnchorPrefix + "abilities", RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Missing, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Ability Editor currently provides a lightweight asset-picker entry point and needs a dedicated editor.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.AbilityEditorWindow.Open),
            CreateSection("Vendors", "Configure shops, prices, stock, buy/sell rules, and inventory integration.", typeof(VendorDefinition), "NewVendorDefinition.asset", "Tools/RPG Toolkit/Vendor Editor", EditorToolsDocumentationAnchorPrefix + "vendors", RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Missing, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Vendor Editor currently provides a lightweight asset-picker entry point and needs stock/pricing workflow UI.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.VendorEditorWindow.Open),
            CreateSection("Loot Tables", "Define weighted drops for chests, encounters, rewards, and vendors.", typeof(LootTableDefinition), "NewLootTableDefinition.asset", "Tools/RPG Toolkit/Loot Table Editor", EditorToolsDocumentationAnchorPrefix + "loot-tables", RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Missing, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Loot Table Editor currently provides a lightweight asset-picker entry point and needs weighted-drop editing UI.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.LootTableEditorWindow.Open),
            CreateSection("NPCs", "Create NPC metadata, dialogue links, party hooks, and world-state keys.", typeof(NPCDefinition), "NewNPCDefinition.asset", "Assets/Create/RPG Toolkit/NPC Definition", EditorToolsDocumentationAnchorPrefix + "npcs", RPGToolkitDashboardCapabilityStatus.Missing, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Asset creation and database discovery work; a focused NPC Editor; per-card documentation links are available."),
            CreateSection("Maps", "Author tile layers, zones, entrances, exits, object placements, and transitions.", typeof(RPGMapDefinition), "NewMapDefinition.asset", "Tools/RPG Toolkit/Maps/Map Editor", EditorToolsDocumentationAnchorPrefix + "maps", RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Map authoring has focused tools and validation entry points; deep-link documentation is available.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.MapEditorWindow.Open),
            CreateSection("Tilesets", "Bridge sprite frames to tile metadata, collision defaults, palettes, and map painting.", typeof(RPGTilesetDefinition), "NewTilesetDefinition.asset", "Tools/RPG Toolkit/Maps/Tileset Editor", EditorToolsDocumentationAnchorPrefix + "tilesets", RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Tileset authoring has focused tools and validation entry points; deep-link documentation is available.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.TilesetEditorWindow.Open),
            CreateSection("Sprite Sheets", "Manage source textures, generated frame metadata, tags, groups, and tile defaults.", typeof(RPGSpriteSheetAsset), "NewSpriteSheetAsset.asset", "Tools/RPG Toolkit/Maps/Sprite Sheet Editor", EditorToolsDocumentationAnchorPrefix + "sprite-sheets", RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Sprite sheet authoring has focused tools and validation entry points; deep-link documentation is available.", global::SixStringSyn.RPGToolkit2D.Editor.Windows.SpriteSheetEditorWindow.Open),
            CreateSection("Sprite Sheet Profiles", "Define slicing rules, grid dimensions, naming patterns, pivots, and pixels per unit.", typeof(RPGSpriteSheetProfile), "NewSpriteSheetProfile.asset", "Assets/Create/RPG Toolkit/Foundation/Sprite Sheet Profile", EditorToolsDocumentationAnchorPrefix + "sprite-sheet-profiles", RPGToolkitDashboardCapabilityStatus.Missing, RPGToolkitDashboardCapabilityStatus.Partial, RPGToolkitDashboardCapabilityStatus.Complete, RPGToolkitDashboardCapabilityStatus.Complete, "Profiles support asset creation/runtime use, but no focused profile editor exists yet; deep-link documentation is available.")
        };

        public static IReadOnlyList<RPGToolkitAuthoringSection> Sections => _sections;

        public static IReadOnlyList<RPGToolkitDashboardCapability> Capabilities => _sections.Select(section => section.Capability).ToList();

        private static RPGToolkitAuthoringSection CreateSection(string title, string description, Type assetType, string defaultFileName, string menuPath, string documentationPath, RPGToolkitDashboardCapabilityStatus focusedEditorStatus, RPGToolkitDashboardCapabilityStatus validationStatus, RPGToolkitDashboardCapabilityStatus documentationStatus, RPGToolkitDashboardCapabilityStatus runtimeIntegrationStatus, string notes, Action openEditor = null)
        {
            var capability = new RPGToolkitDashboardCapability(title, assetType, RPGToolkitDashboardCapabilityStatus.Complete, focusedEditorStatus, validationStatus, documentationStatus, runtimeIntegrationStatus, notes);
            return new RPGToolkitAuthoringSection(title, description, assetType, defaultFileName, menuPath, documentationPath, capability, openEditor);
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
