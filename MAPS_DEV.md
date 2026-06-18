# Maps, Tilesets, and Spritesheets Development Plan

This plan defines a full implementation path for making maps, tilesets, and spritesheets first-class RPG Toolkit 2D workflows. It starts from the current partial implementation (`RPGSpriteSheetAsset`, `RPGSpriteSheetProfile`, `RPGTilesetDefinition`, `RPGMapDefinition`, and lightweight editor entry points) and ends with a complete designer-facing workflow for importing art, building tilesets, painting maps, validating content, previewing gameplay metadata, and loading maps at runtime.

## Guiding Goals

- Let designers create RPG maps without leaving the RPG Toolkit workflow.
- Treat spritesheets, tilesets, maps, zones, collisions, entrances, exits, and object placement as connected authoring concepts.
- Preserve stable IDs for every generated or authored entity so save data, quests, dialogue, events, map transitions, and external tools can reference map content safely.
- Support both Unity-native 2D Tilemap rendering and toolkit-owned metadata models.
- Keep deterministic runtime APIs testable without requiring editor-only systems.
- Build each phase with runtime tests, editor tests, documentation, and validation hooks.

## Current Baseline

The repository already contains the following foundations:

- `RPGSpriteSheetAsset` stores a source texture, profile, and frame metadata.
- `RPGSpriteSheetProfile` stores cell size, padding, pivot, pixels-per-unit, and consistency metadata.
- `RPGTilesetDefinition` links to a sprite sheet and stores tile metadata.
- `RPGMapDefinition` stores map size, tileset, layers, zones, and connections.
- Lightweight Map and Tileset editor menu entries can create basic assets.
- Runtime validation exists for several missing or inconsistent map, tileset, and spritesheet conditions.

The missing work is the full end-to-end workflow: import/slice, generate sprite frame metadata, build tilesets, create tile palettes, paint maps, edit layers/zones/objects/connections, preview, validate deeply, load maps at runtime, and document the workflow.

## Phase 0: Scope Lock and Architecture Alignment

### Objective

Define the exact user workflows, runtime/editor boundaries, asset schemas, and compatibility rules before adding large editor systems.

### Implementation Steps

1. Inventory current map, tileset, spritesheet, dashboard, validation, and package documentation code.
2. Decide which parts are runtime-safe and which parts are editor-only.
3. Define the canonical asset chain:
   - `Texture2D` source art.
   - `RPGSpriteSheetProfile` import/slicing rules.
   - `RPGSpriteSheetAsset` frame metadata.
   - `RPGTilesetDefinition` tile metadata and palette grouping.
   - `RPGMapDefinition` tile layers, object layers, zones, entrances, exits, and connections.
   - Runtime map instances and loaded scene objects.
4. Define stable ID conventions for sheets, frames, tiles, layers, zones, entrances, exits, map objects, and connections.
5. Define schema version/migration expectations for map-related assets.
6. Write a short architecture note in package documentation explaining the asset chain.

### Acceptance Criteria

- The team has a written asset-chain specification.
- The planned runtime/editor assembly split is clear.
- No editor-only types leak into runtime assemblies.
- Stable ID formats are documented.

### Tests and Checks

- Add compile tests or assembly reference tests if needed.
- Confirm existing runtime tests still pass.

## Phase 1: Spritesheet Import and Metadata Pipeline

### Objective

Turn spritesheet assets from manual metadata containers into a usable import/slicing workflow.

### Runtime Work

1. Extend `RPGSpriteSheetProfile` with fields needed for real slicing:
   - slicing mode: grid, custom rectangles, Unity sprite metadata, or external JSON.
   - margin and spacing.
   - default frame naming pattern.
   - orientation and ordering rules.
   - optional animation grouping rules.
   - optional collision/default metadata rules for tile sheets.
2. Extend `RPGSpriteSheetAsset` frame metadata:
   - source rectangle.
   - normalized UV or pixel rect.
   - tags/categories.
   - frame group or animation clip ID.
   - default tile flags for map use.
   - optional address/key for runtime loading.
3. Add deterministic helper methods:
   - find frame by ID.
   - enumerate frames by tag/group/content kind.
   - validate frame bounds against source texture dimensions.
   - detect duplicate grid positions and duplicate sprite references.

### Editor Work

1. Create a dedicated **Sprite Sheet Editor** window under `Tools > RPG Toolkit > Maps > Sprite Sheet Editor`.
2. Add workflows to:
   - create a profile.
   - create a sprite sheet asset from a selected `Texture2D`.
   - generate frame metadata from the selected profile.
   - sync with Unity `Sprite` slices where available.
   - preview the texture grid and selected frames.
   - bulk rename frames.
   - bulk assign tags, animation keys, pivots, and default tile flags.
3. Add import warnings when the texture import settings are incompatible:
   - wrong texture type.
   - missing multiple sprite mode when Unity slicing is expected.
   - mismatched pixels-per-unit.
   - inconsistent cell dimensions when the profile requires consistency.
4. Add optional one-click fixes for safe import settings.

### Validation Work

Add validation for:

- missing source texture.
- missing profile.
- invalid cell size.
- frame rectangles outside texture bounds.
- duplicate frame IDs.
- duplicate grid positions.
- missing Unity sprite references when required.
- mismatched pixels-per-unit.
- frame IDs that violate naming rules.

### Acceptance Criteria

- A designer can select a texture, create a spritesheet asset, generate frame metadata, preview frames, and save the asset.
- Validation reports actionable errors and warnings.
- Existing manually authored sheets remain compatible.

### Tests and Checks

- Runtime tests for frame lookup, tag queries, group queries, and validation.
- Editor tests for metadata generation from a profile.
- Editor tests for duplicate frame and out-of-bounds diagnostics.

## Phase 2: Tileset Authoring and Tile Palette Generation

### Objective

Make tilesets a productive bridge between spritesheet frames and map painting.

### Runtime Work

1. Expand `RPGTileDefinition` to include:
   - source frame ID.
   - tile category.
   - walk cost.
   - collision shape mode.
   - terrain tags.
   - autotile/wang tile metadata.
   - animation metadata for animated tiles.
   - default layer kind.
   - custom key/value metadata.
2. Add tile groups/palettes to `RPGTilesetDefinition`:
   - palette ID.
   - display name.
   - category/filter tags.
   - ordered tile IDs.
3. Add deterministic APIs:
   - find tile by ID.
   - find tiles by tag/category.
   - get source sprite/frame for tile.
   - validate tile-to-frame references.

### Editor Work

1. Replace the lightweight Tileset Editor with a real **Tileset Editor** window.
2. Add workflows to:
   - create a tileset from a spritesheet.
   - auto-generate tiles from selected frames.
   - drag frames into tile entries.
   - bulk assign collision/blocking, overhead, terrain, and default layer flags.
   - create and reorder palettes.
   - filter tiles by category/tag.
   - preview tile appearance and metadata.
3. Add a tile palette view that can be reused by the Map Editor.
4. Add batch tools:
   - generate IDs from frame names.
   - detect unused frames.
   - detect duplicate tile sprites.
   - copy metadata from selected tile to many tiles.

### Validation Work

Add validation for:

- missing spritesheet.
- unknown frame IDs.
- missing sprites.
- duplicate tile IDs.
- invalid palette references.
- collision settings that conflict with default layer usage.
- animated tiles with missing or unordered frames.
- autotile metadata that is incomplete.

### Acceptance Criteria

- A designer can generate a complete tileset from a spritesheet.
- A designer can create named palettes and use them in the Map Editor.
- Every tile can be traced back to a source spritesheet frame.

### Tests and Checks

- Runtime tests for tile lookup and tile validation.
- Editor tests for tileset generation from spritesheet frames.
- Editor tests for palette validation and duplicate ID detection.

## Phase 3: Map Data Model Expansion

### Objective

Upgrade maps from basic layer/tile containers into full RPG map definitions ready for visual editing and runtime loading.

### Runtime Work

1. Extend map layer data:
   - stable layer ID.
   - display name.
   - render order.
   - visibility flag.
   - lock flag.
   - layer opacity.
   - default layer kind.
   - optional Unity Tilemap target name.
2. Extend map tile placement data:
   - tile ID.
   - position.
   - rotation/flip flags.
   - override collision flag.
   - override metadata.
3. Add object placement data:
   - object ID.
   - prefab reference or addressable key.
   - grid position and world offset.
   - rotation/scale.
   - spawn condition key.
   - persistent state key.
   - object category: NPC, monster, item, chest, door, save point, decoration, custom.
4. Add entrances and exits:
   - entrance ID.
   - exit ID.
   - target map ID/reference.
   - target entrance ID.
   - facing direction.
   - transition kind.
5. Expand zone data:
   - zone display name.
   - priority.
   - payload type.
   - tags.
   - optional condition keys.
6. Add runtime APIs:
   - get all tiles at position.
   - get layer tile at position.
   - enumerate objects in region.
   - get zones at position by kind/tag.
   - resolve entrance/exit/connection.
   - compute blocking from collision layers, tile metadata, and object metadata.

### Migration Work

1. Preserve existing serialized map fields.
2. Add migration helpers to move old layer/tile data into the expanded schema when necessary.
3. Keep public read-only accessors stable where possible.

### Validation Work

Add validation for:

- duplicate layer IDs.
- duplicate object IDs.
- duplicate entrance/exit IDs.
- missing tileset.
- unknown tile IDs.
- tile placements outside bounds.
- object placements outside bounds.
- invalid layer order.
- exits targeting missing maps or missing entrances.
- zones outside bounds.
- overlapping blockers.
- invalid custom metadata keys.

### Acceptance Criteria

- Existing map definitions still validate and load.
- New maps can represent tiles, objects, zones, entrances, exits, and connections.
- Runtime code can query map content without editor dependencies.

### Tests and Checks

- Runtime tests for migrated map data.
- Runtime tests for layer/tile queries.
- Runtime tests for object and zone queries.
- Runtime tests for connection and entrance resolution.

## Phase 4: Visual Map Editor Core

### Objective

Replace the current lightweight Map Editor with a real tilemap editing workflow.

### Editor Work

1. Build a dockable **Map Editor** window with:
   - map selection/creation.
   - tileset and palette selection.
   - central grid canvas.
   - layer list.
   - inspector panel for selected map/layer/tile/object/zone.
   - validation panel.
2. Implement grid rendering:
   - pan and zoom.
   - grid overlay.
   - map bounds overlay.
   - selected cell highlight.
   - tile preview under cursor.
3. Implement basic tools:
   - select.
   - pencil brush.
   - erase.
   - rectangle fill.
   - flood fill.
   - replace tile.
   - stamp selection.
4. Implement layer controls:
   - add/remove/rename/reorder layers.
   - set layer kind.
   - toggle visibility.
   - lock/unlock.
   - duplicate layer.
5. Implement undo/redo using Unity editor undo APIs.
6. Mark edited assets dirty and save reliably.

### UX Work

1. Add keyboard shortcuts for common tools.
2. Add tile brush size and shape settings.
3. Add status bar readouts for grid position, selected tile, active layer, and validation state.
4. Add contextual help links to documentation.

### Acceptance Criteria

- A designer can create a map, choose a tileset, select a palette tile, paint on layers, erase, fill, and save.
- Undo/redo works for painting and layer changes.
- The editor does not corrupt map data when switching maps or tilesets.

### Tests and Checks

- Editor tests for map creation and save operations.
- Editor tests for brush operations on serialized map data.
- Editor tests for undo/redo where practical.
- Manual Unity validation for interactive canvas behavior.

## Phase 5: Zone, Collision, Trigger, and Metadata Painting

### Objective

Make gameplay metadata authorable directly on the map grid.

### Editor Work

1. Add overlay modes:
   - collision.
   - trigger.
   - encounter.
   - weather.
   - lighting.
   - region.
   - spawn.
2. Add zone tools:
   - rectangle zone creation.
   - paint/erase zone cells.
   - resize/move zone bounds.
   - assign payload IDs.
   - assign tags and priorities.
3. Add collision tools:
   - paint blocking cells.
   - derive blocking from tileset metadata.
   - show combined effective collision.
   - report collision conflicts.
4. Add trigger tools:
   - create trigger zones.
   - link triggers to event IDs or command payloads.
5. Add metadata inspector:
   - view effective metadata at selected cell.
   - show tile, layer, collision, zone, object, and connection data together.

### Runtime Work

1. Add or refine APIs for effective collision and metadata queries.
2. Add zone priority resolution rules.
3. Add hooks/events for entering/exiting zones if appropriate for runtime systems.

### Acceptance Criteria

- A designer can paint encounter/weather/lighting/region/spawn zones.
- A designer can visualize effective collision.
- Runtime systems can query authored metadata deterministically.

### Tests and Checks

- Runtime tests for zone priority and metadata lookup.
- Runtime tests for effective blocking.
- Editor tests for zone creation, editing, and validation.

## Phase 6: Object Placement and Map Connections

### Objective

Allow maps to define gameplay objects, entrances, exits, transitions, and linked maps.

### Editor Work

1. Add object placement mode:
   - select prefab/object definition.
   - place on grid.
   - move, duplicate, rotate, delete.
   - snap to grid or allow offset.
2. Add supported object categories:
   - NPC.
   - monster/encounter spawn.
   - chest.
   - item pickup.
   - door.
   - exit/transition.
   - save point.
   - custom prefab.
3. Add entrance/exit authoring:
   - create entrance IDs.
   - create exits linked to target maps and entrances.
   - preview connection arrows or labels.
   - validate reciprocal links where required.
4. Add object inspector fields:
   - display name.
   - prefab/addressable reference.
   - condition keys.
   - persistent state key.
   - custom metadata.
5. Add map connection browser:
   - list outgoing connections.
   - list incoming references where discoverable.
   - jump to target map asset.

### Runtime Work

1. Add map object spawn descriptors.
2. Add runtime map spawning service interfaces:
   - prefab resolver.
   - object state resolver.
   - transition handler.
3. Add connection resolution helpers.

### Acceptance Criteria

- Designers can place common RPG objects on a map.
- Designers can define entrances/exits and connect maps.
- Runtime code can resolve what should spawn and where.

### Tests and Checks

- Runtime tests for object query and spawn descriptor generation.
- Runtime tests for entrance/exit resolution.
- Editor tests for object placement serialization.
- Editor tests for broken connection validation.

## Phase 7: Runtime Map Loader and Unity Tilemap Integration

### Objective

Render and instantiate authored maps in playable scenes.

### Runtime Work

1. Create a `RPGMapLoader` or equivalent component/service.
2. Support loading an `RPGMapDefinition` into:
   - Unity Grid.
   - Unity Tilemap layers.
   - GameObject containers for placed objects.
3. Add a tile resolver that maps `RPGTileDefinition`/sprite data to Unity `TileBase` assets or generated runtime tiles.
4. Add collision generation options:
   - Tilemap Collider 2D integration.
   - generated collider objects.
   - custom collision query only.
5. Add map transition support:
   - enter map at entrance.
   - unload previous map.
   - notify transition listeners.
6. Add object spawning support:
   - instantiate prefabs.
   - skip hidden/collected/disabled objects based on persistent state.
   - call initialization hooks on spawned objects.
7. Add runtime events:
   - map loaded.
   - map unloaded.
   - object spawned.
   - zone entered/exited, if implemented.

### Editor Work

1. Add preview/playtest helpers:
   - generate temporary scene preview.
   - ping created Tilemaps.
   - load selected map in current scene.
2. Add warnings if required Unity 2D Tilemap packages/settings are missing.

### Acceptance Criteria

- A map authored in the Map Editor can render in a Unity scene.
- Collision and object placement can be instantiated.
- A player can enter a map at a named entrance.
- Map transitions can resolve target map and target entrance data.

### Tests and Checks

- Runtime tests for map loader data conversion where possible.
- Play mode tests for basic map loading.
- Play mode tests for transition resolution.
- Manual Unity validation for rendered maps and colliders.

## Phase 8: Dashboard, Validation, and Project-Wide Tooling

### Objective

Make maps, tilesets, and spritesheets discoverable and maintainable from the broader RPG Toolkit workflow.

### Editor Work

1. Add dashboard sections for:
   - Maps.
   - Tilesets.
   - Sprite Sheets.
   - Sprite Sheet Profiles.
2. Add creation buttons and search support for each asset type.
3. Add project-wide validation actions:
   - validate all sprite sheets.
   - validate all tilesets.
   - validate all maps.
   - validate map graph connections.
4. Add duplicate ID reporting across map-related assets.
5. Add repair utilities where safe:
   - regenerate missing frame IDs.
   - regenerate missing tile IDs.
   - remove empty/null entries.
   - rebuild palette ordering.
6. Add menu items for direct creation and editor opening.

### Runtime/Editor Validation Work

1. Add a map validation service that aggregates:
   - spritesheet validation.
   - tileset validation.
   - map validation.
   - cross-asset reference validation.
2. Ensure validation messages use stable diagnostic codes.
3. Make validation output friendly enough for designers to fix issues without reading source code.

### Acceptance Criteria

- The dashboard exposes the full maps/tilesets/spritesheets workflow.
- Project-wide validation catches broken asset chains.
- Designers can navigate from validation messages to the affected assets.

### Tests and Checks

- Editor tests for dashboard section registration.
- Editor tests for project-wide validation aggregation.
- Editor tests for duplicate ID reporting.

## Phase 9: Documentation, Samples, and Tutorials

### Objective

Document and demonstrate the complete workflow from source art to playable map.

### Documentation Work

1. Add or update package docs for:
   - spritesheet profiles.
   - sprite sheet import/generation.
   - tileset creation.
   - palette organization.
   - map painting.
   - zone/collision/trigger painting.
   - object placement.
   - entrances/exits and map transitions.
   - runtime map loading.
   - validation troubleshooting.
2. Add a quick-start tutorial:
   - import a texture.
   - create a spritesheet asset.
   - generate frames.
   - create a tileset.
   - build palettes.
   - paint a map.
   - add collision and encounter zone.
   - add entrance/exit.
   - load map in play mode.
3. Add API examples for runtime queries:
   - blocked cells.
   - zone lookup.
   - placed object enumeration.
   - transition resolution.

### Sample Work

1. Add a small sample tilesheet texture or use an existing package-safe sample asset.
2. Add sample spritesheet profile, spritesheet asset, tileset, and map.
3. Add a sample scene that loads the map.
4. Add sample scripts demonstrating map transitions and zone queries.

### Acceptance Criteria

- A new user can follow documentation to create and play a simple map.
- Samples import cleanly through Unity Package Manager sample workflows.
- Troubleshooting docs cover common validation errors.

### Tests and Checks

- Documentation link checks where practical.
- Package validation confirms sample metadata is present.
- Manual tutorial pass in a clean project.

## Phase 10: Performance, Scale, and Usability Polish

### Objective

Ensure the workflow remains usable for large RPG projects.

### Performance Work

1. Profile editor painting operations on large maps.
2. Optimize serialized tile storage for sparse and dense layers.
3. Add chunking if needed for very large maps.
4. Cache tileset/palette previews.
5. Avoid unnecessary asset database refreshes.
6. Optimize runtime loading for large tile layers and object counts.

### Usability Work

1. Add favorites/recent maps and tilesets.
2. Add palette search.
3. Add minimap/overview panel.
4. Add copy/paste between maps.
5. Add stamp library assets.
6. Add import/export for map data if useful for external tools.
7. Add keyboard shortcut customization.

### Acceptance Criteria

- Large maps remain responsive during common editing operations.
- Runtime loading has measurable performance targets and tests.
- Designers can reuse stamps/palettes across maps.

### Tests and Checks

- Editor performance smoke tests for brush operations.
- Runtime performance smoke tests for map loading/query APIs.
- Manual profiling notes for large-map scenarios.

## Phase 11: Release Hardening and Compatibility

### Objective

Prepare the full maps/tilesets/spritesheets workflow for release.

### Hardening Work

1. Review all public APIs for naming, consistency, and extension points.
2. Add XML docs or API docs for public runtime APIs.
3. Review serialized field names before release to avoid avoidable migrations.
4. Add migration tests for any schema changes introduced during phases.
5. Verify package exports without missing `.meta` files.
6. Validate package in a clean Unity project.
7. Verify no editor-only references exist in runtime assemblies.
8. Confirm all validation diagnostics have stable codes and actionable messages.

### Compatibility Work

1. Support legacy assets created before the full workflow.
2. Add migration menu items if automatic migration is risky.
3. Document breaking changes, if any.
4. Update changelog and upgrade guide.

### Acceptance Criteria

- The full workflow is stable enough for real project use.
- Existing assets can migrate or continue working.
- Tests and documentation cover the released behavior.

### Tests and Checks

- Full runtime test suite.
- Full editor test suite.
- Package validation suite.
- Manual blank-project validation.
- Manual sample import validation.

## Suggested Implementation Order Summary

1. Lock architecture and schemas.
2. Build spritesheet import/generation first.
3. Build tileset and palette authoring second.
4. Expand the map model before building the visual editor.
5. Implement core map painting.
6. Add gameplay overlays and zones.
7. Add object placement and map connections.
8. Add runtime loader and Tilemap integration.
9. Integrate with dashboard validation.
10. Finish docs, samples, performance, and release hardening.

## Definition of Done for the Full Workflow

The maps, tilesets, and spritesheets implementation should be considered complete only when all of the following are true:

- A designer can start from a source texture and create a validated spritesheet asset.
- A designer can generate tiles and palettes from spritesheet frames.
- A designer can create, paint, edit, validate, and save layered maps visually.
- A designer can paint collision, trigger, encounter, weather, lighting, region, and spawn metadata.
- A designer can place gameplay objects and connect maps through entrances/exits.
- A runtime scene can load authored maps into Unity Tilemaps or equivalent rendered objects.
- Runtime systems can query tiles, blockers, zones, objects, entrances, exits, and map metadata.
- Dashboard and project-wide validation expose broken references and incomplete authoring data.
- Documentation and samples demonstrate the complete source-art-to-playable-map workflow.
- Runtime, editor, play mode, package validation, and manual Unity checks cover the shipped behavior.
