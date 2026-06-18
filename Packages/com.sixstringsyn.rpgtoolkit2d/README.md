# RPG Toolkit 2D

RPG Toolkit 2D is a reusable Unity 6 package for building data-driven 2D RPGs. It provides runtime systems, ScriptableObject content definitions, editor tooling, tests, and documentation for multiple RPG styles without forcing one game structure.

## Installation

During development, install the toolkit as an embedded Unity Package Manager package:

1. Copy or clone this repository.
2. Open the Unity project with Unity 6.
3. Confirm `Packages/com.sixstringsyn.rpgtoolkit2d` exists.
4. Open **Window > Package Manager** and verify **RPG Toolkit 2D** appears under embedded or project packages.

For a blank Unity 6 project, add the package from disk by selecting **Package Manager > + > Add package from disk...** and choosing `Packages/com.sixstringsyn.rpgtoolkit2d/package.json`.

## Feature List

- Stable RPG identifiers, tags, validation diagnostics, and definition databases.
- Character definitions, stat blocks, resources, leveling, inventory, equipment, item pickups, and save data.
- Dialogue definitions, conditional branching, commands, presenter/context interfaces, and NPC adapters.
- Quest definitions, objectives, rewards, journal persistence, and dialogue command integration.
- Interactions, scene transitions, world state flags, party roster data, crafting, combat, abilities, status effects, loot, and vendors.
- Editor dashboard, content authoring workflow helpers, inspectors, item database browsing, dialogue graph tooling, quest editing, save/world debuggers, and package validation.
- Runtime and editor tests, release documentation, upgrade notes, and troubleshooting guidance.

## Unity Version

The package targets Unity 6 and declares Unity `6000.0` compatibility in `package.json`.

## First Workflow

Open **Tools > RPG Toolkit > Dashboard** to validate package setup, create core content, browse definitions, and launch focused editor tools. See `Documentation~/getting-started.md` for the first item pickup and save/load milestone.

## Known Limitations

- Economy and crafting APIs are marked experimental for the 1.0.0 release candidate and may receive compatibility-breaking refinements.
- Samples currently include package metadata scaffolding only; production scenes should be supplied by the host project until full sample packs are imported.
- Unity Editor package validation, local-path install, Git URL install, and package-removal checks must be run in the target release environment before publishing.
