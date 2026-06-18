# Editor Tools

RPG Toolkit 2D's editor tools are designed around the dashboard at **Tools > RPG Toolkit > Dashboard**. Use the dashboard as the first stop for project setup, content creation, validation, sample discovery, database browsing, and links to focused authoring windows.

## Dashboard

Menu path: **Tools > RPG Toolkit > Dashboard**

The dashboard includes:

- **Quick Start** guidance for validating setup, creating an authoring folder, and opening documentation.
- **Create RPG Content** cards for characters, items, quests, dialogue, abilities, vendors, loot tables, and NPC definitions.
- **Database Browser** search for authored assets with quick ping/open actions and duplicate RPG ID warnings.
- **Focused Tools** shortcuts for the Quest Editor, Dialogue Graph Editor, Item Database, Save Data Debugger, World State Debugger, and package samples folder.
- **Project Setup and Validation** checks for package foundation files, required packages, recommended sample packages, and the default `Assets/RPGToolkit2D` authoring folder.

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

## Project Setup Validation

The dashboard runs package foundation validation and project-level checks. Required package checks should be treated as blockers. Recommended package checks are informational until you use samples or integrations that depend on those packages.

When a warning appears, read its message first. The dashboard provides friendly guidance such as creating the default authoring folder or installing recommended Unity packages for sample scenarios.
