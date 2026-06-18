# Maps, Tilesets, and Spritesheets Workflow

This guide walks through the complete RPG Toolkit 2D map pipeline, from source art to a playable map loaded at runtime.

## Asset chain

1. **Texture2D source art** - imported as a sprite sheet texture in Unity.
2. **RPGSpriteSheetProfile** - stores slicing rules such as cell size, margin, spacing, naming, pivot, and pixels-per-unit.
3. **RPGSpriteSheetAsset** - stores generated frame metadata, tags, animation groups, source rectangles, and default tile flags.
4. **RPGTilesetDefinition** - maps sprite sheet frames to tile IDs, collision defaults, terrain tags, and palettes.
5. **RPGMapDefinition** - stores tile layers, zones, object placements, entrances, exits, and map connections.
6. **RPGMapLoader** - builds runtime tilemaps, spawns objects, resolves transitions, and exposes map query APIs.

Keep IDs stable once content ships. Save data, quests, dialogue commands, events, and external tools should reference map content by map IDs, tile IDs, layer IDs, zone IDs, object IDs, entrance IDs, and exit IDs instead of by display name.

## Quick-start tutorial

### 1. Import a texture

1. Copy a tilesheet texture into your Unity project, for example `Assets/RPGToolkit2D/Art/DemoTiles.png`.
2. Select the texture and set the import mode expected by your workflow. Grid slicing usually works with a normal 2D sprite texture; Unity-slice synchronization requires **Sprite Mode: Multiple**.
3. Keep pixels-per-unit consistent with the profile you will create.

### 2. Create a spritesheet profile

1. Open **Assets > Create > RPG Toolkit > Foundation > Sprite Sheet Profile**.
2. Set the grid cell size, margin, spacing, default frame naming pattern, pivot, and pixels-per-unit.
3. If the sheet must remain grid-consistent, enable consistency checks so validation catches mismatched cell dimensions.

### 3. Generate a spritesheet asset

1. Open **Tools > RPG Toolkit > Maps > Sprite Sheet Editor**.
2. Select the imported texture and profile.
3. Create or select an `RPGSpriteSheetAsset`.
4. Generate frames from the profile.
5. Review generated frame IDs, source rectangles, tags, frame groups, and default tile flags.
6. Run validation and fix missing profiles, invalid cell sizes, out-of-bounds rectangles, duplicate frame IDs, duplicate grid positions, and pixels-per-unit mismatches.

### 4. Create a tileset

1. Open **Tools > RPG Toolkit > Maps > Tileset Editor**.
2. Select the generated `RPGSpriteSheetAsset`.
3. Generate tile definitions from selected frames.
4. Assign categories such as `Ground`, `Wall`, `Water`, `Overhead`, or project-specific tags.
5. Configure collision defaults, walk cost, terrain tags, animation metadata, autotile metadata, and default layer kinds.
6. Validate missing sprite sheet references, unknown frame IDs, duplicate tile IDs, invalid palette references, and conflicting collision/layer defaults.

### 5. Build palettes

Organize the tileset into palettes that match designer tasks:

- **Ground** for walkable floor and terrain.
- **Collision** for blocking wall, cliff, and water tiles.
- **Decoration** for props and non-blocking detail.
- **Overhead** for tree canopies, bridge tops, and roof edges.
- **Triggers** for invisible or debug trigger tiles.

Keep palettes small enough to scan quickly. Prefer stable tile IDs and palette IDs over reusing display names as references.

### 6. Paint a map

1. Open **Tools > RPG Toolkit > Maps > Map Editor**.
2. Create or select an `RPGMapDefinition`.
3. Assign the tileset.
4. Add layers with explicit layer kind, display name, render order, visibility, lock state, opacity, and optional Unity Tilemap target name.
5. Paint ground first, then collision, decoration, overhead, and trigger layers.
6. Validate layer IDs, duplicate tile positions per layer, unknown tile IDs, map bounds, opacity, and render order.

### 7. Add collision and encounter zones

Use collision layers or tile collision defaults for blockers. Use zones for map metadata that is not a rendered tile:

- **Encounter** zones for random battle tables or spawn payload IDs.
- **Weather** zones for ambience changes.
- **Lighting** zones for post-processing or tint keys.
- **Spawn** zones for dynamic enemy or pickup placement.
- **Region** zones for custom triggers.

Priorities determine which zone wins when systems ask for the highest-priority match at a cell.

### 8. Place objects

Add object placements for NPCs, monsters, items, chests, doors, save points, decorations, and custom prefabs. Configure:

- Stable object ID.
- Prefab or addressable key.
- Grid position plus world offset.
- Rotation and scale.
- Spawn condition key.
- Persistent state key.
- Category and custom metadata.

Use `RPGMapObjectSpawner.BuildSpawnDescriptors` when you need deterministic, testable spawn data before instantiating prefabs.

### 9. Add entrances, exits, and transitions

1. Add an entrance such as `town_south_gate` at the cell where the player appears.
2. Add an exit such as `to_forest` at the transition cell.
3. Assign the target map and target entrance ID.
4. Choose a transition kind such as `Fade`, `Door`, or `Stairs`.
5. Use **Tools > RPG Toolkit > Maps > Connection Browser** to inspect cross-map links.
6. Validate missing target maps, missing target entrances, duplicate entrance IDs, and duplicate exit IDs.

### 10. Load the map in play mode

1. Add an empty GameObject to a scene.
2. Add `RPGMapLoader`.
3. Assign the initial map.
4. Choose a collision mode: `TilemapCollider2D`, `GeneratedColliderObjects`, `QueryOnly`, or `None`.
5. Optionally assign a player transform and custom tile, prefab, state, or transition resolvers from your gameplay code.
6. Enter Play Mode and call `LoadMap(map, entranceId)` or `TryTransition(exitId)`.

## Runtime query examples

```csharp
using UnityEngine;
using SixStringSyn.RPGToolkit2D.Maps;

public sealed class MapQueryExamples : MonoBehaviour
{
    [SerializeField] private RPGMapDefinition map;

    public bool IsBlocked(Vector2Int cell) => map != null && map.IsBlocked(cell);

    public RPGMapZone EncounterZoneAt(Vector2Int cell) =>
        map == null ? null : map.GetHighestPriorityZoneAt(cell, RPGMapZoneKind.Encounter);

    public void LogObjects(RectInt room)
    {
        if (map == null) return;

        foreach (var placement in map.GetObjectsInRegion(room))
        {
            Debug.Log($"{placement.objectId} at {placement.gridPosition} ({placement.category})");
        }
    }

    public bool TryResolveTransition(string exitId, out RPGMapConnectionResolution resolution)
    {
        resolution = map == null ? null : map.ResolveExitConnection(exitId);
        return resolution != null && resolution.IsResolved;
    }
}
```

## Validation troubleshooting

| Symptom | Common cause | Fix |
| --- | --- | --- |
| Sprite sheet reports missing texture or profile | The generated asset was created before selecting source art or slicing rules. | Assign the texture/profile in the Sprite Sheet Editor and regenerate frames. |
| Frame rectangle is outside texture bounds | Cell size, margin, spacing, or ordering does not match the imported texture. | Correct the profile and regenerate metadata. |
| Tileset has unknown frame IDs | Frames were renamed or regenerated after tiles were created. | Regenerate tile IDs from frame names or relink tiles to current frame IDs. |
| Palette references missing tiles | A tile was deleted after palette creation. | Use safe repair to remove missing/duplicate palette entries, then rebuild palette ordering. |
| Map has unknown tile IDs | The assigned tileset changed or the map references an old tileset. | Reassign the expected tileset or repaint affected cells. |
| Exits do not resolve | Target map or target entrance ID is missing. | Open the Connection Browser and create the target entrance before shipping. |
| Runtime loads visually but collision is wrong | Collision layer kind, tile blocking flags, or loader collision mode do not match. | Verify blocking tile metadata and set the loader to `TilemapCollider2D`, `GeneratedColliderObjects`, or `QueryOnly` intentionally. |

## Sample content

Import **Map Workflow Tutorial** from Package Manager samples to get a compact README and scripts for zone queries, transition requests, and runtime loader setup. The sample is intentionally code-first so it stays safe for projects with their own art style; replace the placeholder references with your own texture, spritesheet profile, tileset, and map assets.
