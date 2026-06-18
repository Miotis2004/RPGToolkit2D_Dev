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

Use the **Characters** card to create `CharacterDefinition` assets for player characters, companions, enemies, and stat templates. **Open Tool** launches the Character Editor, where you can search existing characters, create or duplicate assets, ping/select assets, edit core fields, assign role tags such as `party` or `enemy`, maintain stat templates/resources, and review validation for missing display names, duplicate IDs, invalid stat entries, and broken references.

### Items

Use the **Items** card to create `ItemDefinition` assets for consumables, equipment, quest items, pickups, and inventory content. **Open Tool** launches the Item Editor, where authors can search items, filter by type, rarity, and stackability, create or duplicate items, delete with confirmation, ping/select assets, edit item fields in a focused detail panel, validate IDs/display names/stack sizes/references, repair duplicate IDs, and export CSV balance reviews. The dashboard item card also summarizes item counts by type and rarity when expanded.

### Quests

Use the **Quests** card to create `QuestDefinition` assets for objectives, conditions, rewards, and quest state. **Open Tool** launches the Quest Editor for core quest inspection and validation while fuller visual workflow tooling remains in progress.

### Dialogue

Use the **Dialogue** card to create `DialogueDefinition` assets for branching conversations, choices, and command-driven narrative flow. **Open Tool** launches the Dialogue Graph Editor for graph-oriented editing and validation.

### Abilities

Use the **Abilities** card to create `AbilityDefinition` assets for combat abilities, interaction effects, and reusable authored actions. **Open Tool** launches the Ability Editor, where authors can search abilities, create or duplicate definitions, ping/select assets, edit metadata, tags, targeting mode, range, cooldowns, resource costs, and effect lists, and review validation for missing display names, duplicate IDs, invalid costs/cooldowns, and incomplete effect references.

### Vendors

Use the **Vendors** card to create `VendorDefinition` assets for shops, stock, prices, and buy/sell rules. **Open Tool** launches the Vendor Editor, where authors can search vendors, create or duplicate definitions, ping/select assets, edit metadata, tags, sell multipliers, stock item references, prices, and quantities, validate missing items, invalid prices, duplicate stock entries, and open the Item Database for referenced item work.

### Loot Tables

Use the **Loot Tables** card to create `LootTableDefinition` assets for weighted chest drops, encounter rewards, quest rewards, and vendor-related drop lists. **Open Tool** launches the Loot Table Editor, where authors can search tables, create or duplicate definitions, ping/select assets, edit weighted item or nested-table entries, quantity ranges, validate empty tables, missing rewards, invalid weights or ranges, and run deterministic simulation previews for expected drop rates.

### NPCs

Use the **NPCs** card to create `NPCDefinition` assets for metadata, dialogue links, party hooks, relationship data, schedule references, and world-state keys. Click **Open Tool** or choose **Tools > RPG Toolkit > NPC Editor** to manage NPCs in a focused workflow. The editor supports selecting, creating, duplicating, pinging, editing display names/descriptions/tags, assigning character/dialogue/vendor/schedule references, configuring recruitment and world-state keys, showing linked quest objectives, validating missing dialogue and duplicate IDs, and opening the Dialogue Graph or World State Debugger from the selected NPC.

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
| Ability Editor | **Tools > RPG Toolkit > Ability Editor** | Dedicated ability list/detail editor with metadata, targeting, costs, effects, and validation. |
| Loot Table Editor | **Tools > RPG Toolkit > Loot Table Editor** | Dedicated weighted-entry editor with validation and simulation previews. |
| Faction Database Editor | **Tools > RPG Toolkit > Faction Database Editor** | Create and validate faction collections used by reputation-aware dialogue, events, shops, and AI. |
| Skill Tree Designer | **Tools > RPG Toolkit > Skill Tree Designer** | Author skill node prerequisites, point costs, and progression metadata. |
| Advanced Loot Table Editor | **Tools > RPG Toolkit > Advanced Loot Table Editor** | Validate nested weighted loot tables and run seeded drop simulations. |
| Crafting Editor | **Tools > RPG Toolkit > Crafting Editor** | Manage recipe databases, crafting stations, and validation for inventory-backed recipes. |
| Vendor Editor | **Tools > RPG Toolkit > Vendor Editor** | Dedicated stock/pricing editor with validation and Item Database navigation. |
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

## Complete Quest Authoring Guide

Open **Tools > RPG Toolkit > Quest Editor** when building quest content. The upgraded Quest Editor is designed so authors can complete the common workflow without bouncing between the Project window and default Inspector.

### 1. Find or create a quest

Use the **Quest Library** search panel to filter quests by display name, RPG ID, or asset path. Click **Create Quest Asset** to add a new `QuestDefinition`, then select it from the list. The selected quest stays in the detail panel for editing, pinging, and Project selection.

### 2. Fill metadata

In **Metadata**, assign a stable RPG ID, display name, description, and tags. Keep IDs stable after shipping because save files and dialogue/event commands can reference quests by ID.

### 3. Author objectives

Use the reorderable **Objectives** list to add, remove, and drag objectives into the desired journal order. Each objective supports its type, description, item reference for collect objectives, target/world-state key, required amount, and optional flag. Prefer explicit target IDs for talk, reach, kill, escort, craft, and custom event objectives so runtime systems can advance them deterministically.

### 4. Add dependencies and conditions

Use **Start Conditions / Dependencies** for prerequisite quests and custom flags. Quest prerequisites become the quest chain view: conditions referencing other quests show which content must be inactive, active, completed, failed, or turned in before this quest starts. Custom flags can mirror world-state keys used by dialogue, events, or map triggers.

### 5. Add rewards and turn-in behavior

Use the reorderable **Rewards** list for item, experience, currency, and custom-action rewards. Item rewards must reference an item; currency rewards should specify the currency ID and quantity; custom actions should use a stable action key handled by your reward receiver. Enable **Auto Turn In** for quests that should grant rewards as soon as all required objectives complete.

### 6. Review quick links

The **Quick Links** area summarizes target/world-state keys and lists linked NPC, Item, and Dialogue assets where references can be inferred. Use **Ping** to jump to related content while wiring quest objectives to NPC IDs, item references, and dialogue quest commands.

### 7. Validate and repair safely

The **Validation** panel groups current diagnostics by severity. Errors block a valid quest, such as missing objectives or item rewards without items. Warnings flag risky content, such as quests without rewards or objectives without target IDs. Use **Repair** for deterministic safe repairs such as regenerating a missing RPG ID. The dashboard Quests card also reports invalid quest counts plus quests missing objectives or rewards.
