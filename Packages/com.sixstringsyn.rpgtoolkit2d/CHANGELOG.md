# Changelog

All notable changes to RPG Toolkit 2D will be documented in this file.

## [Unreleased]

### Phase 2 Roadmap - Dialogue and Quest Authoring

- Added dialogue authoring metadata for localization keys, portraits, and speaker animations.
- Added quest-update and reward dialogue node types plus command helpers for dialogue-driven quest events.
- Added designer-facing quest objective templates for kill, collect, escort, craft, talk, reach, and custom objectives.
- Expanded runtime tests around dialogue presentation metadata and quest objective templates.


## [1.0.0] - Phase 13 Package Hardening and Release Preparation

### Added

- Added a Package Manager importable Basic RPG Setup sample and package dependency declarations for built-in Unity modules used by runtime systems.
- Promoted package metadata to a `1.0.0` release candidate with Package Manager documentation, changelog, license, and sample metadata links.
- Added release documentation hub, API reference, extension guide, upgrade guide, and troubleshooting page.
- Added an explicit `RPGToolkitExperimentalAttribute` marker and tagged late-phase economy/crafting APIs that still need broader production feedback.
- Added deterministic performance smoke tests for inventory operations, database validation, dialogue traversal, quest tracking, and save/load serialization.

### Changed

- Expanded the README with the final feature list and known limitations for release review.
- Extended package validation to require the release documentation set.

### Release Validation Notes

- Full edit mode/play mode test execution, Unity Package Manager local-path and Git URL installation checks, blank Unity 6 project validation, dependency-combination validation, and clean package-removal checks must be completed in the target Unity release environment before publishing.

## [0.1.0]

### Phase 11 - Editor Dashboard and Authoring Workflow Polish

- Expanded the dashboard into a central authoring hub with content creation cards, searchable database browsing, duplicate ID warnings, focused tool shortcuts, project setup validation, sample/documentation links, editor tests, and editor tools documentation.

### Phase 10 - World State, Party, Companions, NPCs, and Crafting

- Added persistent world state values and conditions, party roster and companion recruitment state, NPC definitions/components, inventory-backed crafting recipes, save contributors, a world state debugger window, tests, and documentation.

### Phase 9 - Abilities, Combat, Status Effects, Loot, and Vendors

- Added reusable combat damage primitives, targeting and hit-detection contracts, ability execution, melee/projectile adapters, status effect runtime, loot tables, vendor shops, and Phase 9 authoring windows.

### Phase 8 - Quest System and Quest Editor

- Added quest definitions with objectives, conditions, rewards, validation, save data, tracker state transitions, and dialogue command integration.
- Added a Quest Editor window for creating, inspecting, and validating quest assets.
- Added runtime and editor tests for quest progression, persistence, validation, and editor availability.



### Phase 7 - Dialogue System and Dialogue Graph Editor

- Added branching dialogue definitions with nodes, choices, conditions, commands, graph validation, a deterministic dialogue runner, presenter and NPC interaction adapters, editor tooling, tests, and documentation.

### Phase 6 - Interaction and Scene Transition Systems

- Added shared interactable contracts, priority selection, prompt hooks, trigger/overlap/raycast detection support, NPC, pickup, door, and trigger interactable components.
- Added scene transition requests, a transition service, spawn points, validation helpers, runtime tests, and documentation.


### Phase 5 - Save and Load System

- Added JSON save game containers, metadata, slot file management, contributor registration, and migration hooks.
- Added inventory and character save contributors plus quest, world, and scene placeholder save data models.
- Added a read-only Save Data Debugger editor window for enumerating save slots.
- Added runtime and editor tests for serialization, graceful failures, version mismatch handling, and slot enumeration.

### Added

- Added Phase 4 item definition fields, item instances, inventory containers, equipment validation, pickup components, inventory save data, item database window, and runtime tests.
 - 2026-06-17

### Added

- Implemented Phase 3 character, stats, leveling, and resources with stat definitions, deterministic stat blocks, modifiers, resource pools, character instances, XP curves, editor inspectors, tests, and documentation.
- Implemented the Phase 2 core data model with stable RPG IDs, base definition assets, tags, validation diagnostics, generic definition databases, duplicate ID detection, tests, and shared systems documentation.
- Created the Phase 1 Unity package shell with `package.json`, README, MIT license, package folders, assembly definitions, placeholder dashboard, package validation utility, and editor foundation tests.
- Recorded Phase 0 repository and package audit decisions.
- Confirmed the Unity development host uses Unity `6000.5.0f1`, which is Unity 6-compatible.
- Established `com.sixstringsyn.rpgtoolkit2d` as the Unity Package Manager identifier.
- Established `SixStringSyn.RPGToolkit2D` as the root namespace for runtime, editor, and test code.
- Selected the MIT License as the initial package license.
- Recorded package author metadata as `Six String Syn`.
- Confirmed the package will be developed in `Packages/com.sixstringsyn.rpgtoolkit2d` and installed through Unity Package Manager as an embedded/local package during development.

### Known Gaps
- `com.unity.cinemachine` and `com.unity.addressables` are required by the development plan but are not currently present in `Packages/manifest.json`.
- Unity Editor validation, package resolution, edit mode tests, and play mode tests still need to be run in an environment with Unity available.
