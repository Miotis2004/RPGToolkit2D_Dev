# Map Workflow Tutorial Sample

This sample demonstrates the Phase 9 documentation workflow in an importable Unity Package Manager sample.

## What it contains

- `Scripts/MapWorkflowSampleController.cs` - a small runtime component that loads a map at an entrance, logs blocked-cell and zone-query results, enumerates placed objects in a region, and requests a transition by exit ID.
- `Scripts/MapTransitionLogger.cs` - a transition handler that logs resolved map transitions before a game-specific scene or fade system takes over.

## How to use

1. Import this sample from **Window > Package Manager > RPG Toolkit 2D > Samples**.
2. Create or assign a texture, `RPGSpriteSheetProfile`, `RPGSpriteSheetAsset`, `RPGTilesetDefinition`, and `RPGMapDefinition` by following `Documentation~/maps-workflow.md`.
3. Create an empty GameObject in a scene and add `RPGMapLoader`.
4. Add `MapWorkflowSampleController` and assign the loader, starting map, entrance ID, optional sample query cell, room bounds, and transition exit ID.
5. Add `MapTransitionLogger` if you want transition requests to print resolved source/target map information instead of immediately loading another scene.
6. Enter Play Mode and review the Console output.

The sample does not include production art. It is designed to import cleanly in any project and to be paired with your own package-safe tilesheet texture and authored map assets.
