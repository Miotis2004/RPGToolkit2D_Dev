# RPG Toolkit Dashboard Development Plan

This plan describes a phased path to complete the missing and partially implemented RPG Toolkit dashboard functionality. It focuses on turning the dashboard from a central launcher and asset creator into a full authoring hub with accurate status, focused editor entry points, per-content documentation, and actionable validation/repair workflows.

## Current Baseline

The dashboard currently provides these working foundations:

- A `Tools > RPG Toolkit > Dashboard` editor window.
- Quick Start actions for creating `Assets/RPGToolkit2D` and opening the editor tools documentation.
- Asset creation cards for Characters, Items, Quests, Dialogue, Abilities, Vendors, Loot Tables, NPCs, Maps, Tilesets, Sprite Sheets, and Sprite Sheet Profiles.
- A database browser with type filtering, search, ping/open actions, and duplicate RPG ID flags.
- Focused tool launch buttons for Quest Editor, Dialogue Graph, Item Database, Save Data Debugger, World State Debugger, Samples Folder, Map Editor, Tileset Editor, Sprite Sheet Editor, and Map Connections.
- Package/setup validation and map workflow validation buttons.

Known gaps to close:

- Content-card `Open Menu/Docs` ignores each section's menu path and only opens the shared editor tools documentation.
- Most content cards do not expose completion status or explain whether their editor is full, partial, or missing.
- Ability, Vendor, and Loot Table editor windows are lightweight asset-picker placeholders.
- Quest and Dialogue tools are useful but not full visual workflow editors.
- Characters and NPCs have asset creation/database support but no focused editor from the dashboard.
- Item tooling lists assets but does not provide full database management/editing from the dashboard.
- Documentation links are shared instead of context-specific deep links.
- Validation is available, but dashboard results are not organized into content-specific “what to fix next” guidance.

## Phase 1: Dashboard Capability Inventory and Status Model

### Goal

Add a first-class capability/status model so the dashboard can accurately show which workflows are complete, partial, or missing.

### Steps

1. Add a dashboard capability model, for example `RPGToolkitDashboardCapability`, with fields for:
   - Content title.
   - Asset type.
   - Asset creation status.
   - Focused editor status.
   - Validation status.
   - Documentation status.
   - Runtime integration status.
   - Notes or remediation text.
2. Define a compact enum such as:
   - `Complete`.
   - `Partial`.
   - `Missing`.
   - `NotApplicable`.
3. Extend `RPGToolkitAuthoringSection` or add a parallel registry with:
   - Dedicated editor menu path.
   - Documentation anchor/path.
   - Completion status metadata.
   - Optional editor window opener delegate.
4. Populate initial statuses based on the current implementation:
   - Complete: dashboard open, asset creation, database search, setup validation, map validation entry points.
   - Partial: Quest Editor, Dialogue Graph, Item Database, Ability Editor, Vendor Editor, Loot Table Editor.
   - Missing: focused Character Editor, focused NPC Editor, per-card tool launch, per-card documentation deep links.
5. Render status chips on every content card.
6. Add a dashboard legend explaining the meaning of each status.
7. Add editor tests that assert every section has status metadata.

### Acceptance Criteria

- Every content card displays a clear implementation status.
- The status data is code-driven, not hard-coded in label text scattered through `OnGUI`.
- Tests fail if a new authoring section is added without status metadata.

## Phase 2: Fix Per-Card Tool and Documentation Actions

### Goal

Make each content card's actions do what the UI says: create assets, open the most relevant focused tool when available, and open context-specific documentation.

### Steps

1. Split `Open Menu/Docs` into two explicit buttons:
   - `Open Tool`.
   - `Open Docs`.
2. Implement a safe menu invocation helper:
   - Check whether a menu item exists or provide an explicit delegate per section.
   - Invoke the tool when it exists.
   - Disable or relabel the button when the tool is missing.
3. Replace shared documentation links with section-specific docs:
   - Characters.
   - Items.
   - Quests.
   - Dialogue.
   - Abilities.
   - Vendors.
   - Loot Tables.
   - NPCs.
   - Maps.
   - Tilesets.
   - Sprite Sheets.
   - Sprite Sheet Profiles.
4. Add anchors to `Documentation~/editor-tools.md` or split into separate documentation pages if the file becomes too large.
5. Update button tooltips to explain what will happen.
6. Add fallback behavior:
   - If a focused tool is missing, open the general docs and show a dashboard warning.
   - If docs are missing, show a warning with the expected path/anchor.
7. Add tests for section metadata:
   - Tool path/delegate exists for complete/partial tools.
   - Docs path exists for every section.

### Acceptance Criteria

- Users can open a relevant editor or see a clear unavailable state for every card.
- Documentation links land on the correct section or page.
- No content card uses a misleading combined `Open Menu/Docs` action.

## Phase 3: Content-Specific Dashboard Cards

### Goal

Turn generic content cards into workflow cards that show useful project information and next steps.

### Steps

1. Add asset counts to each card.
2. Add duplicate ID counts per content type.
3. Add invalid asset counts per content type where validation APIs exist.
4. Add “last validation result” state for each content type.
5. Add quick actions per card:
   - `Create`.
   - `Browse`.
   - `Validate`.
   - `Open Tool`.
   - `Docs`.
6. Support card expansion/collapse to avoid making the dashboard too tall.
7. Add a compact mode for small editor windows.
8. Add warning messages for empty but important content types, such as no maps or no items.
9. Add content-specific setup hints, for example:
   - Maps need a tileset.
   - Tilesets need sprite sheet/frame metadata.
   - Vendors need item stock.
   - NPCs often need dialogue links.
10. Add tests for the card data aggregation layer without requiring full GUI interaction.

### Acceptance Criteria

- Each card communicates what exists, what is broken, and what the next authoring action should be.
- Dashboard layout remains usable when all sections are visible.
- Card data can be unit-tested independently from `OnGUI`.

## Phase 4: Full Character Editor

### Goal

Add a focused Character Editor and wire it into the dashboard.

### Steps

1. Create `CharacterEditorWindow` under the editor windows namespace.
2. Support selecting, creating, duplicating, and pinging `CharacterDefinition` assets.
3. Display and edit core fields through a dedicated UI rather than only relying on the inspector.
4. Add stat-template editing helpers.
5. Add party/enemy role tags if supported by runtime data.
6. Add validation summary for missing display names, duplicate IDs, invalid stats, or broken references.
7. Add search/filter support for existing characters.
8. Add dashboard card integration:
   - `Open Tool` launches the Character Editor.
   - Character card shows counts and validation status.
9. Add editor tests for asset creation and validation workflow.
10. Add documentation for the Character Editor.

### Acceptance Criteria

- Character authoring no longer depends solely on the Project inspector.
- The dashboard can launch the Character Editor directly.
- Character docs and validation are reachable from the dashboard.

## Phase 5: Full NPC Editor

### Goal

Add a focused NPC Editor for metadata, dialogue links, party hooks, schedules, and world-state keys.

### Steps

1. Create `NPCEditorWindow`.
2. Support selecting, creating, duplicating, and pinging `NPCDefinition` assets.
3. Add a dedicated field layout for:
   - Display name.
   - Dialogue definition/reference.
   - World-state keys.
   - Party hooks or recruitment data where supported.
   - Optional schedule references.
4. Add a relationship panel showing linked dialogue and quests where available.
5. Add validation for missing dialogue links, duplicate IDs, and broken references.
6. Add quick action to open linked Dialogue Graph.
7. Add quick action to open linked World State Debugger.
8. Wire `Open Tool` from the NPC dashboard card.
9. Add editor tests for creation, linking, and validation.
10. Add documentation for NPC authoring.

### Acceptance Criteria

- NPCs can be managed from a focused editor launched by the dashboard.
- Missing or broken NPC references are visible without manually inspecting assets.
- Linked dialogue/world-state workflows are easy to reach.

## Phase 6: Upgrade Item Database into an Item Editor

### Goal

Expand item tooling from a read-only list into a full item database/editor workflow.

### Steps

1. Add search, type filter, rarity filter, and stackability filter.
2. Add create, duplicate, delete-with-confirmation, ping, and select actions.
3. Add an editable detail panel for selected `ItemDefinition` assets.
4. Add validation for missing IDs, duplicate IDs, invalid stack sizes, missing display names, and broken references.
5. Add bulk duplicate ID reporting and repair options where safe.
6. Add CSV/JSON export if useful for balancing reviews.
7. Add dashboard card counts by item type and rarity.
8. Add tests for search/filter behavior and validation summaries.
9. Update docs with an Item Database workflow.

### Acceptance Criteria

- Item authors can browse and edit items from one focused tool.
- Dashboard item status reflects actual item database health.
- Validation results are actionable.

## Phase 7: Replace Ability, Vendor, and Loot Table Placeholder Windows

### Goal

Replace lightweight asset-picker windows with dedicated editors for Ability, Vendor, and Loot Table workflows.

### Ability Editor Steps

1. Create a dedicated ability list/detail UI.
2. Add create, duplicate, ping, select, and validate actions.
3. Add editing support for ability metadata, targeting, costs, cooldowns, tags, and effects based on runtime data.
4. Add validation for missing names, duplicate IDs, invalid costs/cooldowns, and broken effect references.
5. Add documentation and dashboard integration.

### Vendor Editor Steps

1. Create a vendor list/detail UI.
2. Add stock table editing with item references, prices, quantities, restock rules, and buy/sell flags.
3. Add validation for missing items, invalid prices, duplicate stock entries, and broken references.
4. Add quick navigation to Item Database for referenced items.
5. Add documentation and dashboard integration.

### Loot Table Editor Steps

1. Create a loot table list/detail UI.
2. Add weighted entry editing with item references, quantity ranges, conditions, and preview rolls.
3. Add validation for missing items, zero/negative weights, invalid quantity ranges, and empty tables.
4. Add simulation preview for expected drop rates.
5. Add documentation and dashboard integration.

### Acceptance Criteria

- None of these workflows rely on the generic `AssetPickerWindow` placeholder.
- Each editor exposes domain-specific fields and validation.
- Dashboard cards report meaningful health for abilities, vendors, and loot tables.

## Phase 8: Quest Editor Workflow Upgrade

### Goal

Evolve the Quest Editor from an inspector wrapper into a full quest-authoring workflow.

### Steps

1. Add a quest list/search panel.
2. Add a dedicated quest detail panel for metadata, objectives, conditions, rewards, and turn-in behavior.
3. Add objective ordering and drag/reorder support.
4. Add quick links to related NPCs, Items, Dialogue, and World State keys.
5. Add a validation panel grouped by severity.
6. Add a quest dependency/chain view.
7. Add safe repair actions for common issues.
8. Update the dashboard quest card with counts for invalid quests and quests missing rewards/objectives.
9. Add editor tests for quest creation, objective editing, reward editing, and validation.
10. Expand documentation with a complete quest authoring guide.

### Acceptance Criteria

- Quest authors can complete the core quest workflow without switching to the default inspector for every operation.
- The dashboard accurately reports quest authoring health.

## Phase 9: Dialogue Graph Workflow Upgrade

### Goal

Turn the current dialogue list/editor into a usable graph-oriented dialogue tool.

### Steps

1. Add a graph canvas or structured node-link view.
2. Add node selection and editable node detail panel.
3. Add explicit edge/choice editing.
4. Add validation for unreachable nodes, missing entry nodes, broken choice targets, duplicate IDs, and invalid quest/reward references.
5. Add quick links to related NPCs, Quests, Items, and World State keys.
6. Add search by speaker, localization key, node text, and command payloads.
7. Add mini-map or node outline for large conversations if using a true graph canvas.
8. Add dashboard counts for invalid dialogue assets and unreachable-node warnings.
9. Add editor tests for node creation, linking, search, and validation.
10. Expand documentation with a dialogue graph authoring guide.

### Acceptance Criteria

- Dialogue can be authored as connected flow rather than only as a list of nodes.
- Broken dialogue links are visible from both the editor and dashboard.

## Phase 10: Validation Center and Repair Workflow

### Goal

Consolidate validation across all content types into a dashboard validation center.

### Steps

1. Add a `Validation Center` section to the dashboard.
2. Group validation by content type.
3. Add severity filters: Errors, Warnings, Info.
4. Add text search across validation messages.
5. Add object ping/select/open actions for each diagnostic.
6. Add safe repair actions with clear previews.
7. Add “Validate All RPG Content” covering all supported content types.
8. Persist last validation results during the editor session.
9. Add export-to-markdown or copy-to-clipboard for validation reports.
10. Add tests for aggregation and filtering.

### Acceptance Criteria

- Users can answer “what is broken in my RPG data?” from one dashboard section.
- Safe repairs are explicit, previewable, and limited to deterministic fixes.

## Phase 11: Dashboard UX Polish

### Goal

Make the dashboard comfortable for daily use in real projects.

### Steps

1. Add tabs or navigation sections:
   - Overview.
   - Create.
   - Database.
   - Validation.
   - Tools.
   - Docs/Samples.
2. Add a project health summary at the top.
3. Add recently used assets/tools.
4. Add favorites/pinned workflows.
5. Add persistent user preferences with `EditorPrefs` or `SessionState`.
6. Add responsive layout behavior for narrow windows.
7. Add icons and color-coded severity/status indicators.
8. Add keyboard shortcuts for search and validation.
9. Add empty-state help for new projects.
10. Add performance guards for large projects, such as cached asset queries and manual refresh.

### Acceptance Criteria

- Dashboard remains readable and fast as project content grows.
- Important actions are discoverable without overwhelming new users.

## Phase 12: Tests, Documentation, and Release Readiness

### Goal

Lock the dashboard implementation down with tests, documentation, and release notes.

### Steps

1. Add editor tests for dashboard metadata completeness.
2. Add editor tests for every content card create action.
3. Add editor tests for tool launch metadata where practical.
4. Add editor tests for database search and duplicate detection.
5. Add editor tests for validation aggregation and filtering.
6. Add tests for each new focused editor's creation and validation workflows.
7. Update `Documentation~/editor-tools.md` with the final dashboard workflow.
8. Add or update content-specific docs for every card.
9. Update `README.md` to describe the completed dashboard.
10. Update `CHANGELOG.md` with dashboard completion notes.
11. Add screenshots or GIFs for documentation if repository policy allows binary docs assets.
12. Run the full Unity editor test suite before release.

### Acceptance Criteria

- All dashboard workflows have documented behavior and test coverage.
- Release notes accurately describe the dashboard improvements.
- The dashboard no longer contains misleading or placeholder UI for advertised workflows.

## Suggested Implementation Order

1. Phase 1: Status model.
2. Phase 2: Correct per-card actions and docs.
3. Phase 3: Rich content cards.
4. Phase 10: Validation Center foundations.
5. Phase 6: Item Editor upgrade.
6. Phase 4: Character Editor.
7. Phase 5: NPC Editor.
8. Phase 7: Ability/Vendor/Loot editors.
9. Phase 8: Quest Editor upgrade.
10. Phase 9: Dialogue Graph upgrade.
11. Phase 11: UX polish.
12. Phase 12: final docs/tests/release readiness.

This order fixes misleading dashboard behavior early, then adds visibility into project health, then upgrades the most important content-authoring workflows one by one.

## Definition of Done for the Dashboard

The dashboard should be considered complete when:

- Every displayed action either works or is clearly disabled with an explanation.
- Every content type has asset creation, browsing, validation, documentation, and an appropriate editor workflow.
- Placeholder asset-picker windows have been replaced or explicitly marked as lightweight helpers.
- Project health can be understood from the dashboard without manually opening every asset type.
- Duplicate IDs, missing references, invalid values, and incomplete workflow data are surfaced with object links and actionable fixes.
- Documentation and tests cover the dashboard's advertised behavior.
