# Future Development Roadmap

This roadmap converts the wishlist into implementation phases that build on each other. The goal is to deliver useful editor-facing functionality early while establishing shared runtime foundations for later systems.

## Phase 0: Foundation and Architecture

**Goal:** Create the shared patterns and data contracts every future feature will depend on.

- Define ScriptableObject/database conventions for toolkit content.
- Establish stable runtime IDs for content references.
- Add editor-window navigation patterns for the future RPG Toolkit Game Builder.
- Create validation and diagnostics utilities for content authors.
- Define serialization/versioning rules for saved content and project data.
- Document extension points for custom conditions, rewards, events, and variables.

**Exit criteria:** New systems can share IDs, validation, editor UX patterns, and serialization conventions instead of inventing their own.

## Phase 1: Core Data and State Backbone

**Goal:** Build the systems that dialogue, quests, events, saves, and cutscenes all require.

### Database System

Centralized searchable databases for:

- Items
- Weapons
- Armor
- NPCs
- Enemies
- Skills
- Quests
- Dialogue

Deliverables:

- Database asset types and registries.
- Search, filter, sort, and reference lookup support.
- Broken-reference detection.
- Bulk validation for designer-authored data.

### Variable and Switch System

RPG Maker-style state support:

- Global variables
- Local variables
- Switches
- Flags

Example state keys:

- `HasMetKing`
- `BridgeRepaired`
- `PlayerReputation`

Deliverables:

- Runtime variable/switch store.
- Editor for declaring keys and default values.
- Condition API for querying variables and switches.
- Change notification hooks for quests, dialogue, and events.

**Exit criteria:** Designers can define content records and game state in reusable places, and other features can query those records consistently.

## Phase 2: Dialogue and Quest Authoring

**Goal:** Deliver the first major game-making workflow: conversations that affect quests and game state.

### Visual Dialogue Editor

Node-based editor flow:

```text
NPC
  |
Choice
  |
Condition
  |
Quest Update
  |
Reward
```

Features:

- Branching dialogue
- Variables
- Quest integration
- Portraits
- Speaker animations
- Localization support

Deliverables:

- Node graph asset format.
- Dialogue runtime player.
- Choice, condition, reward, and quest-update nodes.
- Portrait and speaker metadata.
- Localization key support.

### Quest Designer

Designer-authored quest objectives without coding:

- Kill 10 Slimes
- Collect 5 Herbs
- Talk to NPC
- Reach Location
- Escort NPC
- Craft Item

Deliverables:

- Quest asset model.
- Objective templates.
- Quest state machine.
- Reward integration.
- Dialogue-triggered quest updates.

**Exit criteria:** Designers can create a quest, connect it to dialogue, test it in play mode, and inspect quest/variable state.

## Phase 3: Event System and Save Framework

**Goal:** Make maps interactive and preserve player progress reliably.

### Event System

RPG Maker-style map events:

- Show dialogue
- Move NPC
- Play sound
- Give item
- Start quest
- Open shop
- Change variable
- Teleport player

Deliverables:

- Event command model.
- Visual event editor.
- Runtime event runner.
- Condition and variable integration.
- Hooks for dialogue, quests, inventory, audio, and scene loading.

### Save System Framework

Support:

- Multiple save slots
- Autosave
- Manual save
- Cloud save hooks
- Version migration

Deliverables:

- Save profile and slot model.
- Runtime save/load services.
- System registration API.
- Migration pipeline for post-release data changes.
- Sample save adapters for variables, quests, inventory, and player position.

**Exit criteria:** Interactive events can modify game state, and that state can be saved, loaded, and migrated across versions.

## Phase 4: World Simulation and Presentation

**Goal:** Make worlds feel alive with schedules, map metadata, and directed scenes.

### Tilemap World Toolkit

Provide:

- Map metadata
- Region IDs
- Encounter zones
- Weather zones
- Lighting zones
- Spawn zones

Deliverables:

- Tilemap metadata authoring tools.
- Zone painting/assignment workflow.
- Runtime query API for map regions and zones.
- Encounter, spawn, lighting, and weather hooks.

### NPC Scheduling System

Allow schedules such as:

- 8:00 AM - Home
- 9:00 AM - Market
- 5:00 PM - Tavern
- 10:00 PM - Sleep

Deliverables:

- Schedule assets.
- Time-of-day integration.
- NPC destination/action resolver.
- Event hooks for schedule transitions.

### Cutscene System

Node-based editor for:

- Camera move
- Dialogue
- Animation
- Wait
- Fade
- Branch

Deliverables:

- Cutscene graph asset format.
- Timeline/event bridge where practical.
- Dialogue and variable integration.
- Skippable playback support.

**Exit criteria:** Designers can annotate maps, schedule NPC behavior, and create story sequences that integrate with dialogue, quests, variables, and events.

## Phase 5: Advanced Gameplay Editors

**Goal:** Add differentiating RPG systems that expand the toolkit beyond basic quest/dialogue workflows.

### Reputation and Faction System

Support factions such as:

- Kingdom
- Guild
- Church
- Bandits
- Merchants

Reputation can affect:

- Dialogue
- Prices
- Quests
- Hostility

Deliverables:

- Faction database.
- Reputation value model.
- Modifiers and thresholds.
- Condition integration for dialogue, quests, events, shops, and AI.

### Skill Tree Designer

Visual editor for:

- Warrior
- Mage
- Rogue
- Hybrid classes

Deliverables:

- Skill tree graph assets.
- Unlock rules.
- Cost requirements.
- Runtime progression API.

### Loot Table Editor

Weighted drops:

- Gold
- Weapons
- Armor
- Rare drops
- Quest items

Deliverables:

- Weighted loot table assets.
- Nested loot table support.
- Simulation/testing tools.
- Enemy and chest integration hooks.

### Crafting Editor

Visual recipes such as:

```text
Iron Ore + Coal -> Iron Bar
```

Deliverables:

- Recipe database.
- Ingredient and output rules.
- Crafting station metadata.
- Inventory integration hooks.

**Exit criteria:** Designers can author progression, factions, loot, and crafting through editor tools with minimal or no coding.

## Phase 6: AI Tooling

**Goal:** Provide commercial-grade AI authoring for enemies, NPCs, bosses, and companions.

### Behavior Tree Editor

Use cases:

- Enemy AI
- NPC AI
- Boss AI
- Companions

Deliverables:

- Behavior tree graph editor.
- Runtime behavior tree executor.
- Blackboard integration with variables and local AI state.
- Debug visualization during play mode.
- Reusable action, condition, selector, and sequence nodes.

**Exit criteria:** AI behavior can be authored visually, debugged in play mode, and connected to combat, factions, schedules, and variables.

## Phase 7: RPG Toolkit Game Builder

**Goal:** Make the toolkit feel like a cohesive commercial product rather than a collection of separate windows.

Create a custom Unity editor window at:

```text
Tools > RPG Toolkit
```

Tabs:

- Characters
- Items
- Dialogue
- Quests
- Combat
- Maps
- NPCs
- Events
- Variables
- Save Data
- Settings

Deliverables:

- Unified navigation shell.
- Cross-system search.
- Recent assets and favorites.
- Validation dashboard.
- Content creation wizards.
- Project setup checklist.

**Exit criteria:** A developer can spend hours inside the Game Builder creating and validating RPG content without touching Project folders directly.

## Recommended Implementation Order

1. Database system
2. Variable/switch system
3. Visual dialogue editor
4. Quest designer
5. Event system
6. Save system framework
7. Tilemap world toolkit
8. NPC scheduling
9. Cutscene system
10. Reputation/faction system
11. Skill tree designer
12. Loot table editor
13. Crafting editor
14. Behavior tree editor
15. RPG Toolkit Game Builder shell and polish

## Planning Notes

- Build shared runtime APIs before editor polish so each editor has reliable behavior to target.
- Prefer small vertical slices: data model, editor authoring, runtime playback, validation, and sample content.
- Keep systems modular. Users should be able to adopt dialogue without quests, or quests without tilemaps.
- Prioritize migration and validation early because RPG projects accumulate large amounts of authored content.
- Reuse node graph infrastructure across dialogue, cutscenes, skill trees, and behavior trees where possible.
