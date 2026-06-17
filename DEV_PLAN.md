# RPG Toolkit 2D Development Plan

## 1. Project Vision

RPG Toolkit 2D is a reusable Unity 6 package for building 2D RPGs. The package should be installable through Unity Package Manager and should provide runtime systems, data-driven content definitions, editor tooling, sample scenes, automated tests, and complete documentation.

The package must support multiple RPG styles without forcing a single game structure, including top-down RPGs, action RPGs, turn-based RPGs, tactical RPGs, story-heavy RPGs, monster collectors, survival RPGs, JRPGs, Zelda-likes, and Diablo-likes.

## 2. Package Identity and Repository Layout

### 2.1 Package Name

Use this package identifier unless project ownership changes:

```text
com.sixstringsyn.rpgtoolkit2d
```

### 2.2 Development Project Layout

The Unity project is a package development host. Toolkit code must live under `Packages/com.sixstringsyn.rpgtoolkit2d`, not under `Assets`.

```text
RPGToolkit2D_Dev/
├── Assets/
│   └── TestScenes/
├── Packages/
│   └── com.sixstringsyn.rpgtoolkit2d/
└── ProjectSettings/
```

### 2.3 Package Layout

```text
Packages/com.sixstringsyn.rpgtoolkit2d/
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
├── Runtime/
│   ├── Core/
│   ├── Characters/
│   ├── Stats/
│   ├── Inventory/
│   ├── Equipment/
│   ├── Items/
│   ├── Quests/
│   ├── Dialogue/
│   ├── Combat/
│   ├── Abilities/
│   ├── Saving/
│   ├── World/
│   └── com.sixstringsyn.rpgtoolkit2d.runtime.asmdef
├── Editor/
│   ├── Dashboard/
│   ├── Inspectors/
│   ├── Windows/
│   ├── DialogueGraph/
│   ├── QuestEditor/
│   └── com.sixstringsyn.rpgtoolkit2d.editor.asmdef
├── Tests/
│   ├── Runtime/
│   └── Editor/
├── Samples~/
│   ├── BasicRPG/
│   ├── DialogueDemo/
│   ├── InventoryDemo/
│   ├── QuestDemo/
│   └── CombatDemo/
└── Documentation~/
    ├── index.md
    ├── getting-started.md
    ├── architecture.md
    ├── systems.md
    ├── editor-tools.md
    ├── samples.md
    ├── api.md
    └── extension-guide.md
```

## 3. Architecture Principles

### 3.1 Layering

The toolkit should be split into three conceptual layers.

1. **Core Layer**
   - Pure C# where practical.
   - Contains rules and data structures for stats, items, inventory, quests, dialogue, combat, abilities, and saving.
   - Should be heavily unit tested.
   - Avoid direct scene, UI, and MonoBehaviour dependencies.

2. **Runtime Layer**
   - Unity-facing integrations.
   - Contains ScriptableObjects, MonoBehaviours, scene systems, prefabs, UI adapters, interaction components, and save adapters.
   - Converts Unity objects and scene events into calls against the Core Layer.

3. **Editor Layer**
   - Unity Editor-only tools.
   - Contains custom inspectors, graph editors, database windows, dashboard tools, wizard workflows, and validation utilities.

### 3.2 Data-Driven Content

Most game content should be created through ScriptableObjects and editor windows. Users should be able to create characters, items, quests, dialogue, abilities, vendors, and loot tables without modifying toolkit source code.

### 3.3 Extensibility

Every major system should expose extension points:

- Interfaces for service replacement.
- Events for runtime hooks.
- ScriptableObject definitions for content authoring.
- Serializable state classes for save/load.
- Adapter classes for UI, input, scene transitions, hit detection, and custom game rules.

### 3.4 Testing Requirements

Every phase must include automated tests where practical. Runtime tests should prioritize deterministic core logic. Editor tests should verify asset creation, validation, and editor workflow behavior.

## 4. Required Unity Packages

Install or verify these dependencies early:

| Package | Identifier | Purpose |
| --- | --- | --- |
| Unity Input System | `com.unity.inputsystem` | Player input and interaction bindings. |
| Cinemachine | `com.unity.cinemachine` | Sample camera behavior. |
| Addressables | `com.unity.addressables` | Optional scalable asset loading. |
| AI Navigation | `com.unity.ai.navigation` | NPC and enemy navigation samples. |
| Test Framework | `com.unity.test-framework` | Runtime and editor tests. |
| 2D Tilemap | Built in | Tile-based sample scenes. |

## 5. Milestones and Phases

## Phase 0: Repository and Package Audit

### Goal

Confirm the development project is ready for package development and establish baseline project conventions.

### Implementation Tasks

- Verify Unity version is Unity 6-compatible.
- Confirm the package will be developed under `Packages/com.sixstringsyn.rpgtoolkit2d`.
- Decide license text and package author metadata.
- Confirm required packages in `Packages/manifest.json`.
- Define namespace convention, such as `SixStringSyn.RPGToolkit2D`.
- Define coding standards for runtime, editor, and tests.

### Testing and Validation

- Open the project in Unity 6 without compile errors.
- Run Unity package resolution.
- Run existing edit mode and play mode tests, if any.

### Documentation Instructions

- Add package development assumptions to `Documentation~/architecture.md` once the package folder exists.
- Document the package namespace, Unity version, required dependencies, and supported installation method.
- Record initial decisions in `CHANGELOG.md` under version `0.1.0`.

### Exit Criteria

- Project opens successfully.
- Required dependencies are known.
- Package folder plan is accepted.

## Phase 1: Package Foundation

### Goal

Create a valid Unity package shell that can be installed locally through Package Manager.

### Implementation Tasks

- Create `Packages/com.sixstringsyn.rpgtoolkit2d`.
- Add `package.json` with semantic version `0.1.0`.
- Add `README.md`, `CHANGELOG.md`, and `LICENSE.md`.
- Add `Runtime`, `Editor`, `Tests`, `Samples~`, and `Documentation~` folders.
- Add runtime, editor, runtime test, and editor test assembly definitions.
- Configure editor assembly references so editor code can reference runtime code.
- Add initial package validation utility in the editor layer.
- Add a minimal `RPG Toolkit` menu item under `Tools > RPG Toolkit`.

### Testing and Validation

- Confirm Unity compiles all assemblies.
- Confirm Package Manager recognizes the local package.
- Confirm `Tools > RPG Toolkit` menu item opens a placeholder dashboard.
- Add editor tests for package metadata validation.

### Documentation Instructions

- Package `README.md` must explain package purpose, installation, Unity version, and first workflow.
- `Documentation~/getting-started.md` must explain how to install the package in a blank Unity 6 project.
- `Documentation~/architecture.md` must explain folder structure and assembly layout.
- `CHANGELOG.md` must include the package shell creation entry.

### Exit Criteria

- Package is visible in Package Manager.
- Placeholder dashboard opens.
- Foundation tests pass.

## Phase 2: Core Data Model

### Goal

Build shared identifiers, database abstractions, tags, and base definitions used by all RPG systems.

### Implementation Tasks

- Implement `RPGId` as a stable serializable identifier.
- Implement `RPGObject` or base definition class for content assets.
- Implement `RPGTag` and tag query utilities.
- Implement `RPGDatabase<T>` for locating and validating definitions.
- Implement validation result models for editor and runtime diagnostics.
- Create base ScriptableObject definitions:
  - Character definition.
  - Item definition.
  - Quest definition.
  - Dialogue definition.
  - Ability definition.
- Add asset creation menu items.
- Add duplicate ID detection.

### Testing and Validation

- Unit test ID generation, equality, serialization, and duplicate detection.
- Editor test ScriptableObject asset creation.
- Editor test database validation output.

### Documentation Instructions

- Document core identifiers in `Documentation~/architecture.md`.
- Add a `Documentation~/systems.md` section for shared concepts.
- Add API examples showing how to create and reference definitions.

### Exit Criteria

- All base definition assets can be created.
- Databases can scan and validate assets.
- Duplicate IDs are reported clearly.

## Phase 3: Character, Stats, Leveling, and Resources

### Goal

Create the character foundation for player characters, NPCs, companions, and enemies.

### Implementation Tasks

- Implement stat definitions and stat value types.
- Implement `StatBlock` with base values and calculated values.
- Implement additive, multiplicative, override, temporary, and conditional modifiers.
- Implement resource pools for health, mana, stamina, or custom resources.
- Implement character definitions with display name, level, portrait, stat template, tags, and optional prefab references.
- Implement character instances separate from definitions.
- Implement XP curves and level-up rules.
- Implement runtime events for stat changes, level changes, and resource changes.
- Add editor inspectors for stat and character definitions.

### Testing and Validation

- Unit test stat calculations and modifier order.
- Unit test resource clamping, damage, healing, and regeneration.
- Unit test XP thresholds and level-up behavior.
- Editor test character asset creation and validation.

### Documentation Instructions

- Add a character system guide to `Documentation~/systems.md`.
- Document modifier calculation order.
- Include examples for creating a player character, enemy, and companion.
- Add troubleshooting notes for missing stat definitions and invalid XP curves.

### Exit Criteria

- Character definitions and instances work independently.
- Stats and resource pools are deterministic and tested.
- Character assets can be authored without code.

## Phase 4: Items, Inventory, Equipment, and Pickups

### Goal

Implement the first complete playable workflow: create an item, pick it up, add it to inventory, save it, and load it.

### Implementation Tasks

- Implement item definitions with name, description, icon, tags, stack size, rarity, and item type.
- Implement item instance data for durability, generated stats, custom state, and quantity.
- Implement stackable items, consumables, weapons, armor, quest items, and generic items.
- Implement inventory container add, remove, move, split, merge, and query operations.
- Implement equipment slots and equipment validation.
- Implement item use behavior through interfaces or ScriptableObject actions.
- Implement pickup components for scene items.
- Implement inventory events for UI adapters.
- Add editor item database window.

### Testing and Validation

- Unit test add, remove, stack, split, merge, overflow, and invalid operation cases.
- Unit test equipment slot restrictions.
- Play mode test pickup interaction adds an item to inventory.
- Save/load tests must verify inventory state round trips.

### Documentation Instructions

- Add an inventory guide to `Documentation~/systems.md`.
- Document item definition fields and recommended item categories.
- Document item use extension points.
- Add a step-by-step tutorial for creating a pickup and verifying inventory contents.

### Exit Criteria

- Users can create an item in the editor.
- A pickup can add that item to an inventory.
- Inventory can be saved and loaded.
- Inventory tests pass.

## Phase 5: Save and Load System

### Goal

Provide stable JSON save/load infrastructure for toolkit systems.

### Implementation Tasks

- Implement save slots.
- Implement serializable game state containers.
- Implement per-system save contributors.
- Implement inventory persistence.
- Implement character persistence.
- Implement quest and world state persistence placeholders.
- Implement scene state persistence foundation.
- Add save metadata such as timestamp, playtime, version, and scene name.
- Add migration hooks for future save version upgrades.
- Add save data debugger editor window.

### Testing and Validation

- Unit test JSON serialization and deserialization.
- Unit test missing, corrupted, and version-mismatched save files.
- Play mode test save/load of inventory and character state.
- Editor test save debugger can enumerate slots.

### Documentation Instructions

- Add a save system guide to `Documentation~/systems.md`.
- Document save file format at a high level without promising permanent internal field names before `1.0.0`.
- Document how systems register save contributors.
- Add migration guidance for package users.

### Exit Criteria

- Multiple save slots work.
- Inventory and character state round trip successfully.
- Corrupted save data fails gracefully.

## Phase 6: Interaction and Scene Transition Systems

### Goal

Standardize how players interact with NPCs, pickups, doors, triggers, and scene transitions.

### Implementation Tasks

- Implement interactable interface.
- Implement interaction detection adapters for trigger, raycast, and overlap-based interactions.
- Implement NPC interaction component.
- Implement pickup interaction component.
- Implement door and trigger interaction components.
- Implement interaction prompt UI hooks.
- Implement scene transition requests and adapters.
- Implement spawn point definitions for scene entry.
- Add validation for missing interaction labels and missing target scenes.

### Testing and Validation

- Unit test interaction priority and filtering.
- Play mode test pickup, NPC talk, and door interaction flows.
- Play mode test scene transition request generation without requiring full production scenes.

### Documentation Instructions

- Add an interaction guide to `Documentation~/systems.md`.
- Document required components for interactables.
- Document UI prompt adapter implementation.
- Document scene transition setup and spawn points.

### Exit Criteria

- Multiple interactable types use one player-facing interaction flow.
- Scene transitions can be requested through toolkit APIs.
- Prompt UI can be connected by users.

## Phase 7: Dialogue System and Dialogue Graph Editor

### Goal

Provide branching NPC dialogue with choices, conditions, events, and an editor graph.

### Implementation Tasks

- Implement dialogue definitions.
- Implement dialogue nodes, lines, choices, entry points, and exit points.
- Implement condition evaluation.
- Implement dialogue events and commands.
- Implement dialogue runner.
- Implement NPC dialogue adapter.
- Implement UI adapter interface for custom dialogue presentation.
- Build dialogue graph editor with node creation, linking, validation, and search.
- Add dialogue preview tooling.

### Testing and Validation

- Unit test branching, conditions, event firing, and end states.
- Editor test graph asset creation and validation.
- Play mode test NPC starts dialogue and choices advance state.

### Documentation Instructions

- Add a dialogue authoring guide.
- Document node types, choices, conditions, and events.
- Include a tutorial for creating a simple branching conversation.
- Document UI adapter requirements.

### Exit Criteria

- A user can author dialogue visually.
- Dialogue can be run from NPC interaction.
- Conditions and events are tested.

## Phase 8: Quest System and Quest Editor

### Goal

Implement trackable quests with objectives, states, conditions, rewards, and editor tooling.

### Implementation Tasks

- Implement quest definitions.
- Implement quest states: inactive, active, completed, failed, and turned in.
- Implement objective types: collect item, talk to NPC, reach location, kill target, custom event, and custom script.
- Implement quest tracker.
- Implement quest rewards for items, XP, currency, and custom actions.
- Implement quest conditions.
- Connect dialogue events to quest progression.
- Add quest editor window.
- Add quest validation for unreachable objectives and missing rewards.

### Testing and Validation

- Unit test state transitions.
- Unit test objective progress and completion.
- Unit test reward granting.
- Play mode test dialogue starts and advances a quest.
- Editor test quest editor creates valid quest assets.

### Documentation Instructions

- Add a quest system guide.
- Document objective types and custom objective extension points.
- Add a tutorial for a fetch quest and a talk-to-NPC quest.
- Document how dialogue and quest systems integrate.

### Exit Criteria

- Quests can be authored, started, progressed, completed, and rewarded.
- Quest state persists through save/load.
- Quest editor supports normal authoring flow.

## Phase 9: Abilities, Combat, Status Effects, Loot, and Vendors

### Goal

Provide flexible combat primitives that support action, turn-based, tactical, and hybrid RPGs.

### Implementation Tasks

- Implement damage model with damage types, defense, resistance, critical hits, and mitigation hooks.
- Implement ability definitions with costs, cooldowns, range, targeting, effects, and tags.
- Implement ability runtime executor.
- Implement melee attack adapter.
- Implement projectile attack adapter.
- Implement hit detection abstraction so users can plug in physics, grid, or turn systems.
- Implement status effects with duration, stacking, periodic effects, and removal rules.
- Implement enemy AI hooks without locking users into one AI framework.
- Implement loot tables and weighted drops.
- Implement vendor/shop definitions, stock, pricing rules, buy/sell operations, and restock hooks.
- Add combat tuning editor, ability editor, loot table editor, and vendor editor.

### Testing and Validation

- Unit test damage calculations.
- Unit test ability cost, cooldown, target validation, and effect application.
- Unit test status effect stacking and expiration.
- Unit test loot weighting with deterministic random seeds.
- Unit test vendor buy/sell validation.
- Play mode test simple attack and loot drop flow.

### Documentation Instructions

- Add combat, ability, status effect, loot, and vendor guides.
- Document how to adapt combat for action, turn-based, or tactical projects.
- Document hit detection adapter contracts.
- Include examples for a sword attack, fireball, poison effect, loot chest, and shopkeeper.

### Exit Criteria

- Toolkit provides reusable combat primitives.
- Combat is not tied to one presentation or control style.
- Loot and vendor flows integrate with inventory.

## Phase 10: World State, Party, Companions, NPCs, and Crafting

### Goal

Complete the remaining RPG world systems and connect them to save/load, dialogue, quests, inventory, and combat.

### Implementation Tasks

- Implement world state keys, flags, counters, and variables.
- Implement condition checks against world state.
- Implement world state debugger editor window.
- Implement NPC definitions and runtime NPC components.
- Implement party roster and active party selection.
- Implement companion relationship and recruitment hooks.
- Implement crafting recipes, ingredients, station requirements, costs, outputs, and validation.
- Implement crafting UI adapter hooks.
- Connect world state to dialogue, quest, vendor, and scene systems.

### Testing and Validation

- Unit test world state set/get/clear and condition evaluation.
- Unit test party add/remove/active selection.
- Unit test companion recruitment conditions.
- Unit test crafting recipe validation and output generation.
- Save/load tests for world state and party state.

### Documentation Instructions

- Add guides for world state, NPCs, party, companions, and crafting.
- Document world state naming conventions.
- Include examples for a recruited companion, locked door flag, and crafting station.
- Document save/load behavior for these systems.

### Exit Criteria

- World state can drive content conditions.
- Party and companion state persists.
- Crafting integrates with inventory.

## Phase 11: Editor Dashboard and Authoring Workflow Polish

### Goal

Make the editor toolkit the central user-facing value of the package.

### Implementation Tasks

- Expand dashboard with sections for characters, items, quests, dialogue, abilities, vendors, loot tables, saves, world state, samples, settings, and validation.
- Add creation wizards for common assets.
- Add database browsers with search, filters, duplicate checks, and quick-open actions.
- Add project setup validator.
- Add sample import shortcuts.
- Add documentation links from each editor page.
- Add friendly error messages and fix buttons where practical.

### Testing and Validation

- Editor test dashboard opens without exceptions.
- Editor test creation wizards create valid assets.
- Editor test validator detects known misconfiguration fixtures.
- Manual test dashboard workflow in a blank project.

### Documentation Instructions

- Add `Documentation~/editor-tools.md`.
- Document every editor window and its menu path.
- Include screenshots once UI is stable.
- Add a dashboard quick-start tutorial.

### Exit Criteria

- Users can create most RPG content without writing code.
- Dashboard provides a clear path for setup, creation, validation, and samples.

## Phase 12: Samples, Tutorials, and Documentation Completion

### Goal

Ship practical samples and complete user-facing documentation.

### Implementation Tasks

- Build `BasicRPG` sample with player, item pickup, inventory, save/load, NPC, dialogue, quest, combat encounter, loot, and scene transition.
- Build `InventoryDemo` sample focused on items, stacking, equipment, and pickups.
- Build `DialogueDemo` sample focused on branching dialogue and conditions.
- Build `QuestDemo` sample focused on objectives and rewards.
- Build `CombatDemo` sample focused on abilities, damage, status effects, and loot.
- Add sample import metadata to `package.json`.
- Add sample-specific README files.
- Finish API reference.
- Finish extension guide.
- Add troubleshooting and FAQ pages.

### Testing and Validation

- Import each sample into a clean Unity 6 project.
- Run each sample scene manually.
- Run automated runtime and editor test suites.
- Verify documentation links from Package Manager and dashboard.

### Documentation Instructions

- `Documentation~/index.md` must be a navigation hub.
- `Documentation~/getting-started.md` must walk through the first milestone.
- `Documentation~/samples.md` must explain each sample and expected interactions.
- `Documentation~/api.md` must summarize public namespaces and main extension points.
- `Documentation~/extension-guide.md` must explain how to customize systems without modifying package source.

### Exit Criteria

- Every sample imports and runs.
- Documentation covers install, first workflow, each system, editor tooling, samples, API, extension, and troubleshooting.

## Phase 13: Package Hardening and Release Preparation

### Goal

Prepare the package for a stable public release.

### Implementation Tasks

- Review public API naming and namespace consistency.
- Mark experimental APIs explicitly.
- Add XML documentation comments for public APIs.
- Run package validation.
- Verify semantic versioning.
- Confirm license compliance for all assets and samples.
- Optimize editor windows for large content databases.
- Add performance tests for inventory, database validation, dialogue traversal, quest tracking, and save/load.
- Prepare release notes.

### Testing and Validation

- Run full edit mode and play mode test suites.
- Test installation through Package Manager using local path and Git URL.
- Test in a fresh blank Unity 6 project.
- Test in a project that already has Input System, Cinemachine, Addressables, and AI Navigation installed.
- Test package removal leaves no unexpected project assets outside imported samples.

### Documentation Instructions

- Update `CHANGELOG.md` with complete release notes.
- Update package `README.md` with final feature list and known limitations.
- Add upgrade notes for any breaking changes.
- Verify every documentation code sample compiles or is clearly marked as pseudocode.

### Exit Criteria

- Package can be installed, used, tested, and removed cleanly.
- Public documentation is complete.
- Release notes are ready.

## 6. First Practical Milestone

The first practical milestone should prove a complete vertical slice rather than many incomplete systems.

### User Workflow

1. Install package into a blank Unity 6 project.
2. Open `Tools > RPG Toolkit`.
3. Create a character.
4. Create an item.
5. Place a pickup in the scene.
6. Interact with or walk over the pickup.
7. Add the item to inventory.
8. Save the game.
9. Reload the game.
10. Confirm the inventory still contains the item.

### Systems Required

- Package foundation.
- Dashboard placeholder.
- Character definition.
- Item definition.
- Inventory container.
- Pickup interaction.
- JSON save/load.
- Minimal documentation.
- Unit tests for inventory and save/load.
- Play mode test for pickup to inventory.

## 7. Documentation Standards

Every phase must treat documentation as part of the implementation, not a later cleanup task.

### Required Documentation Types

- **Concept guide:** Explains what the system is and when to use it.
- **Authoring guide:** Explains how to create content in the editor.
- **Runtime guide:** Explains required components and scene setup.
- **API guide:** Explains extension points and public types.
- **Tutorial:** Provides a step-by-step practical workflow.
- **Troubleshooting:** Lists common setup mistakes and fixes.

### Documentation Checklist Per System

For each major system, document:

- Purpose.
- Main runtime types.
- Main ScriptableObject definitions.
- Main editor tools.
- Setup steps.
- Save/load behavior.
- Events and extension points.
- Example workflow.
- Tests that protect the system.
- Known limitations.

### Screenshots

Add screenshots for editor workflows after the UI stabilizes. Screenshots should be updated whenever dashboard layout, graph tools, database tools, or setup flows change materially.

## 8. Definition of Done

A phase is complete only when:

- Runtime implementation is complete for that phase's promised scope.
- Editor authoring support exists where required.
- Automated tests cover core behavior.
- Manual Unity validation has been performed for user-facing workflows.
- Documentation has been updated.
- Changelog has been updated.
- Public APIs are named consistently and are discoverable.

## 9. Suggested Version Roadmap

| Version | Target Scope |
| --- | --- |
| `0.1.0` | Package shell, dashboard placeholder, core definitions, item pickup, inventory, save/load vertical slice. |
| `0.2.0` | Characters, stats, resources, leveling, improved item and equipment workflows. |
| `0.3.0` | Interaction, scene transitions, dialogue runtime, dialogue graph MVP. |
| `0.4.0` | Quest system, quest editor, dialogue and quest integration. |
| `0.5.0` | Abilities, combat primitives, status effects, loot, vendors. |
| `0.6.0` | World state, party, companions, NPC tooling, crafting. |
| `0.7.0` | Dashboard polish, database tooling, validation, sample import workflows. |
| `0.8.0` | Complete samples and documentation pass. |
| `0.9.0` | API hardening, performance, compatibility testing, release candidate. |
| `1.0.0` | Stable public release. |

## 10. Major Risks and Mitigations

| Risk | Mitigation |
| --- | --- |
| Scope creep across too many RPG genres. | Build small extensible primitives and defer genre-specific behavior to adapters and samples. |
| Editor tools become hard to maintain. | Keep editor tools thin; move validation and model logic into tested runtime/core services. |
| Save data breaks between versions. | Add save version metadata and migration hooks before broad system adoption. |
| Combat design becomes too opinionated. | Implement abstract damage, targeting, and hit detection contracts rather than one fixed battle system. |
| Samples hide required setup complexity. | Documentation must include blank-project setup tutorials independent of sample scenes. |
| ScriptableObject IDs become unstable. | Use explicit stable IDs and validate duplicates in editor tooling. |

## 11. Immediate Next Actions

1. Create the package folder under `Packages/com.sixstringsyn.rpgtoolkit2d`.
2. Add package metadata, assemblies, README, changelog, license, and documentation shell.
3. Add the placeholder dashboard menu item.
4. Add package validation tests.
5. Implement the first vertical slice: character definition, item definition, pickup, inventory, and JSON save/load.
