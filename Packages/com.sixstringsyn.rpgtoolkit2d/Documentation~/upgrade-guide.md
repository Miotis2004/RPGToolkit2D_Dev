# Upgrade Guide

## 0.1.0 to 1.0.0

No breaking runtime data migrations are required. The package version was promoted to `1.0.0` for release preparation, package metadata now exposes documentation/changelog/license links, and late-phase economy/crafting APIs are explicitly marked experimental.

Recommended checks after upgrading:

1. Run the dashboard package validation.
2. Run edit mode and play mode tests.
3. Review any compile warnings related to experimental APIs before shipping production code that depends on them.

## Map Workflow Release Compatibility (Phase 11)

The maps, tilesets, and spritesheets workflow is hardened for the `1.0.0` release schema. Runtime map assets continue to call `MigrateExpandedMapData()` during serialization so legacy layers, object metadata lists, default object scale, and performance settings are normalized before validation or loading.

Release validation now treats diagnostic codes as a compatibility surface. If you add a map, tileset, or spritesheet validator, register its code in `RPGMapReleaseCompatibility.StableDiagnosticCodes` and provide a designer-facing message that explains what to fix. Use `RPGMapReleaseCompatibility.ValidateMapForRelease(map)` before shipping content packs to confirm migration still succeeds and diagnostics remain stable.

Package validation also checks that runtime scripts do not reference `UnityEditor` APIs and that package files outside `Samples~` have matching `.meta` files. These checks are intended to catch export issues before importing the package into a blank Unity project.
