# RPG Toolkit 2D Architecture

## Phase 0 Package Development Assumptions

RPG Toolkit 2D is developed as an embedded Unity package inside this Unity 6 development host. Toolkit source, tests, samples, and package documentation belong under `Packages/com.sixstringsyn.rpgtoolkit2d` rather than under `Assets`.

## Unity Version

The development host targets Unity `6000.5.0f1`, as recorded in `ProjectSettings/ProjectVersion.txt`. Package code should remain Unity 6-compatible and avoid APIs that require a newer editor unless the package metadata and documentation are updated in the same change.

## Package Identity

- Package identifier: `com.sixstringsyn.rpgtoolkit2d`
- Display name: `RPG Toolkit 2D`
- Release semantic version: `1.0.0`
- Author name: `Six String Syn`
- License: MIT License
- Supported installation method during development: Unity Package Manager embedded/local package from `Packages/com.sixstringsyn.rpgtoolkit2d`

## Namespace Convention

Use `SixStringSyn.RPGToolkit2D` as the root namespace.

Recommended namespace segments:

- Runtime code: `SixStringSyn.RPGToolkit2D.Runtime`
- Core runtime logic: `SixStringSyn.RPGToolkit2D.Runtime.Core`
- Editor tooling: `SixStringSyn.RPGToolkit2D.Editor`
- Runtime tests: `SixStringSyn.RPGToolkit2D.Tests.Runtime`
- Editor tests: `SixStringSyn.RPGToolkit2D.Tests.Editor`

System-specific namespaces should append the system name after the layer, such as `SixStringSyn.RPGToolkit2D.Runtime.Inventory` or `SixStringSyn.RPGToolkit2D.Editor.QuestEditor`.

## Required Dependencies

The development plan identifies these required Unity dependencies:

| Dependency | Package identifier | Current Phase 0 status |
| --- | --- | --- |
| Unity Input System | `com.unity.inputsystem` | Present in `Packages/manifest.json` at `1.19.0`. |
| Cinemachine | `com.unity.cinemachine` | Missing from `Packages/manifest.json`; add before camera samples are implemented. |
| Addressables | `com.unity.addressables` | Missing from `Packages/manifest.json`; add before scalable asset-loading samples are implemented. |
| AI Navigation | `com.unity.ai.navigation` | Present in `Packages/manifest.json` at `2.0.13`. |
| Test Framework | `com.unity.test-framework` | Present in `Packages/manifest.json` at `1.7.0`. |
| 2D Tilemap | `com.unity.modules.tilemap` | Present in `Packages/manifest.json` at `1.0.0`. |

## Package Layout Convention

The package should use the following structure as it is built out in later phases:

```text
Packages/com.sixstringsyn.rpgtoolkit2d/
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
├── Runtime/
├── Editor/
├── Tests/
├── Samples~/
└── Documentation~/
```

Runtime systems should be implemented in `Runtime`, Unity Editor-only tools in `Editor`, automated tests in `Tests`, imported sample content in `Samples~`, and package documentation in `Documentation~`.

## Coding Standards

- Keep deterministic domain rules in plain C# classes where practical.
- Keep Unity-facing behavior in explicit adapters, `MonoBehaviour` components, or `ScriptableObject` definitions.
- Keep Editor-only APIs in editor assemblies and namespaces only.
- Prefer data-driven content definitions over hard-coded game content.
- Use interfaces for replaceable services and events for runtime hooks.
- Avoid direct dependencies from core logic to scenes, UI, input, cameras, or save-file storage.
- Write tests for deterministic core logic and editor validation workflows when each system is added.
- Name files after the primary type they contain.
- Use PascalCase for public types, methods, and properties; camelCase for locals and parameters; and `_camelCase` for private instance fields.

## Phase 1 Assembly Layout

The Phase 1 package foundation defines four assemblies:

| Assembly | Path | Purpose |
| --- | --- | --- |
| `SixStringSyn.RPGToolkit2D.Runtime` | `Runtime/com.sixstringsyn.rpgtoolkit2d.runtime.asmdef` | Runtime and core code available to player builds. |
| `SixStringSyn.RPGToolkit2D.Editor` | `Editor/com.sixstringsyn.rpgtoolkit2d.editor.asmdef` | Editor-only tooling that references the runtime assembly. |
| `SixStringSyn.RPGToolkit2D.Tests.Runtime` | `Tests/Runtime/com.sixstringsyn.rpgtoolkit2d.tests.runtime.asmdef` | Runtime/play mode test code that references runtime code. |
| `SixStringSyn.RPGToolkit2D.Tests.Editor` | `Tests/Editor/com.sixstringsyn.rpgtoolkit2d.tests.editor.asmdef` | Editor test code that references runtime and editor code. |

The editor assembly is constrained to the Unity Editor platform so editor-only APIs are not included in player builds. Runtime code must not reference editor assemblies.

## Phase 2 Core Data Model

Phase 2 adds shared runtime primitives that every RPG system can reuse.

- `RPGId` is a serializable, stable identifier wrapper used to reference content definitions across systems and save data.
- `RPGObject` is the base `ScriptableObject` definition type. It stores an `RPGId`, display name, description, and shared tags.
- `RPGTag` stores normalized content labels, while `RPGTagQuery` provides common has-one, has-any, and has-all checks.
- `RPGDatabase<T>` indexes definition assets by `RPGId` and validates content collections for empty or duplicate identifiers.
- `RPGValidationResult` and `RPGValidationMessage` provide runtime-safe diagnostics that editor tools can display without coupling core code to UnityEditor APIs.

The initial base content definitions are `CharacterDefinition`, `ItemDefinition`, `QuestDefinition`, `DialogueDefinition`, and `AbilityDefinition`. Each can be created through **Assets > Create > RPG Toolkit** menu entries.
