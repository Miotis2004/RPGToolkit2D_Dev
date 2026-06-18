# Editor Tools

RPG Toolkit 2D's editor tools are designed around the dashboard at **Tools > RPG Toolkit > Dashboard**. Use the dashboard as the first stop for project setup, content creation, validation, sample discovery, database browsing, and links to focused authoring windows.

## Dashboard

Menu path: **Tools > RPG Toolkit > Dashboard**

The dashboard includes:

- **Quick Start** guidance for validating setup, creating an authoring folder, and opening documentation.
- **Create RPG Content** cards for characters, items, quests, dialogue, abilities, vendors, loot tables, NPC definitions, maps, tilesets, sprite sheets, and sprite sheet profiles.
- **Database Browser** search for authored assets with quick ping/open actions and duplicate RPG ID warnings.
- **Focused Tools** shortcuts for the Quest Editor, Dialogue Graph Editor, Item Database, Save Data Debugger, World State Debugger, Map Editor, Tileset Editor, Sprite Sheet Editor, Map Connections, and package samples folder.
- **Project Setup and Validation** checks for package foundation files, required packages, recommended sample packages, and the default `Assets/RPGToolkit2D` authoring folder.
- **Maps, Tilesets, and Sprite Sheets** project-wide validation for sprite sheets, tilesets, maps, map graph connections, duplicate map workflow IDs, and safe repair utilities.

## Authoring Workflow Quick Start

1. Open **Tools > RPG Toolkit > Dashboard**.
2. Click **Create Authoring Folder** to create `Assets/RPGToolkit2D`.
3. Use a content card such as **Create Characters** or **Create Items** to create the first asset.
4. Search the asset in the **Database Browser** and confirm it has a unique RPG ID.
5. Open a focused tool when a system needs a deeper workflow, such as the Quest Editor for objectives or the Dialogue Graph for branching conversations.
6. Review **Project Setup and Validation** before importing samples or sharing a project.

## Content Creation Cards

The dashboard provides creation shortcuts for the common ScriptableObject definitions shipped by the toolkit:

| Card | Created asset | Use for |
| --- | --- | --- |
| Characters | `CharacterDefinition` | Heroes, enemies, companions, level/stat templates. |
| Items | `ItemDefinition` | Consumables, equipment, quest items, pickups, inventory content. |
| Quests | `QuestDefinition` | Objectives, conditions, rewards, and quest state. |
| Dialogue | `DialogueDefinition` | Branching conversations, choices, and dialogue commands. |
| Abilities | `AbilityDefinition` | Combat abilities and reusable authored effects. |
| Vendors | `VendorDefinition` | Shops, stock, prices, and buy/sell rules. |
| Loot Tables | `LootTableDefinition` | Weighted drops for chests, encounters, rewards, and shops. |
| NPCs | `NPCDefinition` | NPC metadata, dialogue links, relationship hooks, and world-state integration. |
| Maps | `RPGMapDefinition` | Tile layers, zones, entrances, exits, object placements, and transitions. |
| Tilesets | `RPGTilesetDefinition` | Tile metadata, collision defaults, source frame references, and palette grouping. |
| Sprite Sheets | `RPGSpriteSheetAsset` | Source texture frame metadata, tags, groups, and default tile flags. |
| Sprite Sheet Profiles | `RPGSpriteSheetProfile` | Slicing rules, cell size, ordering, naming, pivots, and pixels per unit. |


### Characters

Use the **Characters** card to create `CharacterDefinition` assets for player characters, companions, enemies, and stat templates. A focused Character Editor is planned; until then, create the asset from the card and edit it in the Inspector or locate it through the Database Browser.

### Items

Use the **Items** card to create `ItemDefinition` assets for consumables, equipment, quest items, pickups, and inventory content. **Open Tool** launches the Item Database so you can browse item assets and check authored IDs before deeper item editing workflows are added.

### Quests

Use the **Quests** card to create `QuestDefinition` assets for objectives, conditions, rewards, and quest state. **Open Tool** launches the Quest Editor for core quest inspection and validation while fuller visual workflow tooling remains in progress.

### Dialogue

Use the **Dialogue** card to create `DialogueDefinition` assets for branching conversations, choices, and command-driven narrative flow. **Open Tool** launches the Dialogue Graph Editor for graph-oriented editing and validation.

### Abilities

Use the **Abilities** card to create `AbilityDefinition` assets for combat abilities, interaction effects, and reusable authored actions. **Open Tool** launches the current lightweight Ability Editor entry point.

### Vendors

Use the **Vendors** card to create `VendorDefinition` assets for shops, stock, prices, and buy/sell rules. **Open Tool** launches the current lightweight Vendor Editor entry point while richer pricing and stock management workflows are planned.

### Loot Tables

Use the **Loot Tables** card to create `LootTableDefinition` assets for weighted chest drops, encounter rewards, quest rewards, and vendor-related drop lists. **Open Tool** launches the current lightweight Loot Table Editor entry point.

### NPCs

Use the **NPCs** card to create `NPCDefinition` assets for metadata, dialogue links, party hooks, relationship data, and world-state keys. A focused NPC Editor is planned; until then, use the Inspector and Database Browser.

### Maps

Use the **Maps** card to create `RPGMapDefinition` assets for tile layers, zones, entrances, exits, object placements, and transitions. **Open Tool** launches the Map Editor, and the validation buttons can check maps independently or as part of the full map-content pipeline.

### Tilesets

Use the **Tilesets** card to create `RPGTilesetDefinition` assets that bridge sprite sheet frames to tile metadata, collision defaults, palette grouping, and map-painting data. **Open Tool** launches the Tileset Editor.

### Sprite Sheets

Use the **Sprite Sheets** card to create `RPGSpriteSheetAsset` assets for source texture frame metadata, tags, groups, and default tile flags. **Open Tool** launches the Sprite Sheet Editor.

### Sprite Sheet Profiles

Use the **Sprite Sheet Profiles** card to create `RPGSpriteSheetProfile` assets for slicing rules, grid dimensions, naming patterns, pivots, and pixels per unit. A focused profile editor is planned; until then, edit profiles in the Inspector and use them from sprite sheet tooling.

## Database Browser

The Database Browser filters assets by type and search text. Search matches asset name, display name, asset path, and RPG ID. Each row includes:

- The asset object field.
- Its project path or a **Duplicate ID** warning.
- **Ping** to locate the asset in the Project window.
- **Open** to select the asset for inspection.

Use duplicate ID warnings before committing authored content. Stable unique IDs are required for save data, quests, dialogue commands, references, and external tools.

## Focused Editor Windows

| Window | Menu path | Purpose |
| --- | --- | --- |
| Dashboard | **Tools > RPG Toolkit > Dashboard** | Central setup, creation, validation, database search, samples, and docs. |
| Item Database | **Tools > RPG Toolkit > Item Database** | Item-focused database view and validation support. |
| Quest Editor | **Tools > RPG Toolkit > Quest Editor** | Create and inspect quest definitions, objectives, rewards, and validation output. |
| Dialogue Graph Editor | **Tools > RPG Toolkit > Dialogue Graph Editor** | Edit dialogue graph assets and validate branching links. |
| Ability Editor | **Tools > RPG Toolkit > Ability Editor** | Lightweight ability asset creation entry point. |
| Loot Table Editor | **Tools > RPG Toolkit > Loot Table Editor** | Lightweight loot table creation entry point. |
| Faction Database Editor | **Tools > RPG Toolkit > Faction Database Editor** | Create and validate faction collections used by reputation-aware dialogue, events, shops, and AI. |
| Skill Tree Designer | **Tools > RPG Toolkit > Skill Tree Designer** | Author skill node prerequisites, point costs, and progression metadata. |
| Advanced Loot Table Editor | **Tools > RPG Toolkit > Advanced Loot Table Editor** | Validate nested weighted loot tables and run seeded drop simulations. |
| Crafting Editor | **Tools > RPG Toolkit > Crafting Editor** | Manage recipe databases, crafting stations, and validation for inventory-backed recipes. |
| Vendor Editor | **Tools > RPG Toolkit > Vendor Editor** | Lightweight vendor creation entry point. |
| Combat Tuning Editor | **Tools > RPG Toolkit > Combat Tuning Editor** | Placeholder for combat tuning workflows. |
| Save Data Debugger | **Tools > RPG Toolkit > Save Data Debugger** | Inspect save slots and serialized save metadata. |
| World State Debugger | **Tools > RPG Toolkit > World State Debugger** | Inspect and edit world-state keys during play and authoring. |
| Map Editor | **Tools > RPG Toolkit > Maps > Map Editor** | Author map layers, tiles, objects, zones, entrances, exits, and validation overlays. |
| Tileset Editor | **Tools > RPG Toolkit > Maps > Tileset Editor** | Generate and inspect tile metadata and palettes from sprite sheet frames. |
| Sprite Sheet Editor | **Tools > RPG Toolkit > Maps > Sprite Sheet Editor** | Generate and inspect frame metadata from textures and profiles. |
| Map Connection Browser | **Tools > RPG Toolkit > Maps > Connection Browser** | Review authored exits and map graph validation. |

## Project Setup Validation

The dashboard runs package foundation validation and project-level checks. Required package checks should be treated as blockers. Recommended package checks are informational until you use samples or integrations that depend on those packages.

When a warning appears, read its message first. The dashboard provides friendly guidance such as creating the default authoring folder or installing recommended Unity packages for sample scenarios.

## Maps, Tilesets, and Sprite Sheets Validation

Use **Validate All Map Content** from the dashboard to aggregate sprite sheet, tileset, map, map graph, cross-asset reference, and duplicate ID diagnostics. Use the narrower validation buttons when fixing one part of the asset chain. Validation rows include the affected asset, a stable diagnostic code, designer-facing guidance, and a **Ping** button for navigating to the asset. **Repair Safe Issues** only performs deterministic cleanup: it fills missing frame/tile IDs, removes null entries, and rebuilds palette ordering by removing missing or duplicate tile references.
