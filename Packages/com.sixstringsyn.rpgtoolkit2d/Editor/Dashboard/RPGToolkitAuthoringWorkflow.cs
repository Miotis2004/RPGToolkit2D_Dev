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
using SixStringSyn.RPGToolkit2D.Runtime.NPCs;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using SixStringSyn.RPGToolkit2D.Runtime.Vendors;
using SixStringSyn.RPGToolkit2D.Editor;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Dashboard
{
    public sealed class RPGToolkitAuthoringSection
    {
        public RPGToolkitAuthoringSection(string title, string description, Type assetType, string defaultFileName, string menuPath, string documentationPath)
        {
            Title = title;
            Description = description;
            AssetType = assetType;
            DefaultFileName = defaultFileName;
            MenuPath = menuPath;
            DocumentationPath = documentationPath;
        }

        public string Title { get; }
        public string Description { get; }
        public Type AssetType { get; }
        public string DefaultFileName { get; }
        public string MenuPath { get; }
        public string DocumentationPath { get; }
    }

    public sealed class RPGToolkitAssetBrowserEntry
    {
        public RPGToolkitAssetBrowserEntry(RPGObject asset, string path, bool duplicateId)
        {
            Asset = asset;
            Path = path;
            DuplicateId = duplicateId;
        }

        public RPGObject Asset { get; }
        public string Path { get; }
        public bool DuplicateId { get; }
    }

    public static class RPGToolkitAuthoringWorkflow
    {
        public const string DefaultAssetFolder = "Assets/RPGToolkit2D";
        public const string EditorToolsDocumentationPath = RPGToolkitPackageValidator.PackagePath + "/Documentation~/editor-tools.md";

        private static readonly IReadOnlyList<RPGToolkitAuthoringSection> _sections = new List<RPGToolkitAuthoringSection>
        {
            new RPGToolkitAuthoringSection("Characters", "Create heroes, enemies, party members, and stat templates.", typeof(CharacterDefinition), "NewCharacterDefinition.asset", "Assets/Create/RPG Toolkit/Character Definition", EditorToolsDocumentationPath),
            new RPGToolkitAuthoringSection("Items", "Create consumables, equipment, quest items, and inventory content.", typeof(ItemDefinition), "NewItemDefinition.asset", "Assets/Create/RPG Toolkit/Item Definition", EditorToolsDocumentationPath),
            new RPGToolkitAuthoringSection("Quests", "Author objectives, conditions, rewards, and turn-in behavior.", typeof(QuestDefinition), "NewQuestDefinition.asset", "Tools/RPG Toolkit/Quest Editor", EditorToolsDocumentationPath),
            new RPGToolkitAuthoringSection("Dialogue", "Build branching conversations and command-driven narrative flow.", typeof(DialogueDefinition), "NewDialogueDefinition.asset", "Tools/RPG Toolkit/Dialogue Graph Editor", EditorToolsDocumentationPath),
            new RPGToolkitAuthoringSection("Abilities", "Create reusable abilities for combat, interactions, and custom effects.", typeof(AbilityDefinition), "NewAbilityDefinition.asset", "Tools/RPG Toolkit/Ability Editor", EditorToolsDocumentationPath),
            new RPGToolkitAuthoringSection("Vendors", "Configure shops, prices, stock, buy/sell rules, and inventory integration.", typeof(VendorDefinition), "NewVendorDefinition.asset", "Tools/RPG Toolkit/Vendor Editor", EditorToolsDocumentationPath),
            new RPGToolkitAuthoringSection("Loot Tables", "Define weighted drops for chests, encounters, rewards, and vendors.", typeof(LootTableDefinition), "NewLootTableDefinition.asset", "Tools/RPG Toolkit/Loot Table Editor", EditorToolsDocumentationPath),
            new RPGToolkitAuthoringSection("NPCs", "Create NPC metadata, dialogue links, party hooks, and world-state keys.", typeof(NPCDefinition), "NewNPCDefinition.asset", "Assets/Create/RPG Toolkit/NPC Definition", EditorToolsDocumentationPath)
        };

        public static IReadOnlyList<RPGToolkitAuthoringSection> Sections => _sections;

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
                var asset = AssetDatabase.LoadAssetAtPath(path, section.AssetType) as RPGObject;
                if (asset == null) continue;
                if (!MatchesSearch(asset, path, search)) continue;
                if (!asset.Id.IsEmpty) ids[asset.Id] = ids.TryGetValue(asset.Id, out var count) ? count + 1 : 1;
                entries.Add(new RPGToolkitAssetBrowserEntry(asset, path, false));
            }

            return entries.Select(entry => new RPGToolkitAssetBrowserEntry(entry.Asset, entry.Path, !entry.Asset.Id.IsEmpty && ids.TryGetValue(entry.Asset.Id, out var count) && count > 1)).ToList();
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

        public static void OpenDocumentation(string path) => EditorUtility.OpenWithDefaultApp(path);

        private static bool MatchesSearch(RPGObject asset, string path, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return true;
            return asset.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || asset.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || asset.Id.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
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
