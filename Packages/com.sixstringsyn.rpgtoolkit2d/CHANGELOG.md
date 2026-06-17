# Changelog

All notable changes to RPG Toolkit 2D will be documented in this file.

## [0.1.0]

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
