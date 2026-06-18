# API Reference

The package uses the `SixStringSyn.RPGToolkit2D` root namespace. Runtime APIs live below `SixStringSyn.RPGToolkit2D.Runtime`; editor-only APIs live below `SixStringSyn.RPGToolkit2D.Editor`.

## Stable runtime namespaces

- `Runtime.Core` contains stable identifiers, tags, validation results, base definition objects, and generic definition databases.
- `Runtime.Items`, `Runtime.Inventory`, and `Runtime.Equipment` contain item definitions, item instances, inventory containers, slots, equipment slots, and save data.
- `Runtime.Characters` and `Runtime.Stats` contain character definitions, instances, stat blocks, modifiers, resource pools, and progression curves.
- `Runtime.Dialogue` contains dialogue definitions, choices, conditions, commands, runners, context interfaces, and presenter interfaces.
- `Runtime.Quests` contains quest definitions, objectives, rewards, runtime instances, journal save data, and quest tracking.
- `Runtime.Saving` contains save game containers, contributors, results, and slot services.
- `Runtime.Interactions` and `Runtime.World` contain interactable contracts, detector helpers, pickup/NPC/door components, world state, spawn points, and scene transition services.

## Experimental APIs

Economy-adjacent and late-phase systems that need broader production feedback are marked with `RPGToolkitExperimentalAttribute`. Treat APIs carrying this attribute as release-candidate quality rather than fully stable.

## Editor APIs

Editor classes are intended for tooling composition and package validation. Do not reference editor assemblies from runtime code.

## Code samples

Documentation snippets are intentionally short and use real public types. If a snippet omits Unity object creation or scene setup for readability, it is marked as pseudocode in the surrounding text.
