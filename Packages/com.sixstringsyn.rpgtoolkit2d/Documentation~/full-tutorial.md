# RPG Toolkit 2D Full Tutorial

This tutorial walks through a complete first-game workflow for RPG Toolkit 2D: installing the Unity package, validating the setup, creating data assets, wiring a small scene, adding maps, dialogue, quests, inventory, combat-adjacent content, saving, and preparing your project for extension.

The goal is not to prescribe one final game architecture. RPG Toolkit 2D is intentionally data-driven: you create ScriptableObject definitions for RPG content, attach runtime components where scene behavior is needed, and use editor windows to validate and browse the result.

## 1. Prerequisites

Before you start, prepare the following:

- Unity 6 (`6000.0` or newer compatible Unity 6 editor).
- A 2D Unity project, preferably with the 2D template.
- This repository cloned locally, or this package copied into your project.
- Basic Unity familiarity: Project window, Inspector, Package Manager, ScriptableObjects, prefabs, scenes, and Play Mode.

Recommended project folders in your consuming game project:

```text
Assets/
  RPG/
    Characters/
    Dialogue/
    Items/
    Maps/
    Quests/
    Stats/
    UI/
  Scenes/
  Scripts/
```

Keep package files under `Packages/com.sixstringsyn.rpgtoolkit2d` unchanged when possible. Put game-specific assets under `Assets/` so package upgrades remain straightforward.

## 2. Install the package

### Option A: Use this development repository

1. Open this repository root in Unity 6.
2. Wait for Unity to import the package.
3. Confirm the package folder exists at `Packages/com.sixstringsyn.rpgtoolkit2d`.
4. Open **Window > Package Manager**.
5. Select **In Project** and verify **RPG Toolkit 2D** is listed.

### Option B: Add the package to another Unity project from disk

1. Open your target Unity 6 project.
2. Open **Window > Package Manager**.
3. Click **+**.
4. Choose **Add package from disk...**.
5. Select `Packages/com.sixstringsyn.rpgtoolkit2d/package.json` from this repository.
6. Confirm **RPG Toolkit 2D** appears in Package Manager.

### Option C: Embed the package

For active package development, copy `Packages/com.sixstringsyn.rpgtoolkit2d` into the target project's `Packages/` folder. Unity treats this as an embedded package, making local edits immediately available.

## 3. Import the onboarding samples

The package includes two Package Manager samples:

- **Basic RPG Setup**: a lightweight checklist for the first content-authoring milestone.
- **Map Workflow Tutorial**: scripts and sample support for loading maps, querying map zones and blockers, enumerating placed objects, and logging transitions.

To import them:

1. Open **Window > Package Manager**.
2. Select **RPG Toolkit 2D**.
3. Expand **Samples**.
4. Import **Basic RPG Setup** first.
5. Import **Map Workflow Tutorial** when you reach the map sections below.

Unity imports samples into `Assets/Samples/RPG Toolkit 2D/<version>/...`, so you can inspect or modify them without changing package source files.

## 4. Open the main tools

Most workflows begin from the toolkit menu:

- **Tools > RPG Toolkit** or **Tools > RPG Toolkit > Game Builder**: guided project builder.
- **Tools > RPG Toolkit > Dashboard**: validation, workflow shortcuts, and tool launch points.
- **Tools > RPG Toolkit > Item Database**: item browsing.
- **Tools > RPG Toolkit > Dialogue Graph Editor**: dialogue authoring.
- **Tools > RPG Toolkit > Quest Editor**: quest authoring.
- **Tools > RPG Toolkit > Save Data Debugger**: inspect save payloads.
- **Tools > RPG Toolkit > World State Debugger**: inspect world flags and values.
- **Tools > RPG Toolkit > Maps > ...**: sprite sheet, tileset, map, connection, and runtime preview tools.

Start by opening **Tools > RPG Toolkit > Dashboard** and running any available validation actions. Resolve missing package, sample, or setup warnings before building content.

## 5. Create your first content database

The database asset is a central registry for RPG definitions. It helps editor tools find content and helps your game keep references organized.

1. In the Project window, create `Assets/RPG/Database/`.
2. Right-click the folder.
3. Choose **Create > RPG Toolkit > Data > Content Database**.
4. Name it `RPGContentDatabase.asset`.
5. Select the asset and keep it open in the Inspector.

As you create definitions below, add them to the database if the Inspector exposes lists for that content type. If your game uses multiple databases, split them by feature or content pack, such as `CoreDatabase`, `Chapter01Database`, and `DemoDatabase`.

## 6. Define stats and resources

Stats represent numeric character properties such as strength, defense, speed, or intellect. Resources represent pools such as health, mana, stamina, or morale.

### Create stat definitions

1. Create `Assets/RPG/Stats/`.
2. Right-click and choose **Create > RPG Toolkit > Stats > Stat Definition**.
3. Create these example stats:
   - `Strength`
   - `Defense`
   - `Agility`
   - `Magic`
4. Give each stat a stable ID, display name, description, and sensible default value if those fields are shown.

### Create resource definitions

1. In `Assets/RPG/Stats/`, right-click and choose **Create > RPG Toolkit > Stats > Resource Definition**.
2. Create:
   - `Health`
   - `Mana`
   - `Stamina`
3. Configure starting and maximum values according to your game's balance.

Tips:

- Use stable IDs such as `stat.strength` and `resource.health` if the Inspector exposes ID fields.
- Avoid renaming IDs after content is referenced by saves, quests, or dialogue conditions.
- Keep display names player-facing and IDs developer-facing.

## 7. Create a playable character definition

Character definitions hold reusable character data. A runtime character instance uses the definition and tracks current mutable state.

1. Create `Assets/RPG/Characters/`.
2. Right-click and choose **Create > RPG Toolkit > Character Definition**.
3. Name it `Hero.asset`.
4. Configure:
   - Stable ID: `character.hero`.
   - Display name: `Hero`.
   - Base stats: add the stat definitions created earlier.
   - Resources: add health, mana, and/or stamina.
   - Leveling: assign an experience curve if your project uses progression.
5. If you need an experience curve, right-click and choose **Create > RPG Toolkit > Characters > Experience Curve**.

For party-based RPGs, repeat this process for companions. Later, register the characters with the party roster system or your own party UI.

## 8. Create items and inventory content

Items are ScriptableObject definitions. Inventories hold item instances and quantities at runtime.

### Create basic items

1. Create `Assets/RPG/Items/`.
2. Right-click and choose **Create > RPG Toolkit > Item Definition**.
3. Create:
   - `Potion.asset`
   - `IronSword.asset`
   - `LeatherArmor.asset`
   - `AncientKey.asset`
4. Configure each item:
   - Stable ID: for example `item.potion`.
   - Display name and description.
   - Stack settings for consumables and crafting materials.
   - Type/category if available.
   - Use behavior if available.

### Create equipment slots

1. Create `Assets/RPG/Items/EquipmentSlots/`.
2. Right-click and choose **Create > RPG Toolkit > Equipment > Equipment Slot**.
3. Create slots such as:
   - `Weapon`
   - `Armor`
   - `Accessory`
4. Configure equipment items to use the intended slot if the item Inspector supports equipment fields.

### Add inventory to a scene object

1. Create or select a `Player` GameObject in your scene.
2. Add the toolkit inventory component if your project uses the built-in `InventoryComponent`.
3. Configure capacity and starting contents in the Inspector where available.
4. Add any equipment container component or your own bridge script if equipment should be active in Play Mode.

Suggested milestone: enter Play Mode, add a potion to the inventory through your test script or debug UI, and verify the item appears in your inventory UI or debug logs.

## 9. Add pickups and interaction

Interactions let scene objects respond to player input. Pickups connect an item definition to an interactable scene object.

### Create a pickup

1. Create a `Potion Pickup` GameObject in your test scene.
2. Add a 2D collider and mark it as trigger if your detector expects trigger interactions.
3. Add the toolkit pickup component or pickup interaction component.
4. Assign `Potion.asset` and quantity `1`.
5. Add a visible sprite.

### Add interaction detection to the player

1. Select the `Player` GameObject.
2. Add a 2D collider and Rigidbody2D appropriate for your controller.
3. Add the toolkit interaction detector component if using built-in detection.
4. Configure detection radius, layer mask, and prompt target if those fields are available.
5. Add or implement an interaction prompt UI by using the prompt interface expected by the detector.

### Test the pickup loop

1. Enter Play Mode.
2. Move the player near the potion.
3. Trigger interaction using your input bridge.
4. Confirm the item enters inventory and the pickup disappears or marks itself collected.

If you do not yet have input/UI code, create a small project-local MonoBehaviour that calls the detector's interaction method when a key is pressed. Keep this bridge in `Assets/Scripts/`, not in the package.

## 10. Build a dialogue conversation

Dialogue definitions contain nodes, choices, conditions, and commands. A presenter controls how text and choices appear in your UI.

### Create dialogue data

1. Create `Assets/RPG/Dialogue/`.
2. Right-click and choose **Create > RPG Toolkit > Dialogue Definition**.
3. Name it `VillageElderDialogue.asset`.
4. Open **Tools > RPG Toolkit > Dialogue Graph Editor**.
5. Select the dialogue definition.
6. Create a start node with greeting text.
7. Add two choices:
   - `Who are you?`
   - `Do you need help?`
8. Connect each choice to a response node.
9. Add an end node or return path.

### Add dialogue to an NPC

1. Create `Assets/RPG/NPCs/`.
2. Right-click and choose **Create > RPG Toolkit > NPC Definition**.
3. Name it `VillageElder.asset`.
4. Assign the dialogue definition where available.
5. In your scene, create a `Village Elder` GameObject.
6. Add a collider, sprite, and NPC interaction or dialogue adapter component.
7. Assign the NPC or dialogue definition.

### Implement or connect a presenter

The runtime dialogue runner is UI-agnostic. Your project should provide a presenter that displays lines and choices. A typical presenter:

1. Receives speaker name and dialogue text.
2. Shows choice buttons.
3. Calls back with the selected choice.
4. Closes the panel when the dialogue ends.

Keep the visual UI in your game project so each RPG can have a different style.

## 11. Create a quest

Quests use definitions for static data and runtime trackers for player progress.

1. Create `Assets/RPG/Quests/`.
2. Right-click and choose **Create > RPG Toolkit > Quest Definition**.
3. Name it `FindTheHerbs.asset`.
4. Configure:
   - Stable ID: `quest.find_the_herbs`.
   - Title: `Find the Herbs`.
   - Description: `Collect three healing herbs for the village elder.`
   - Objectives: collect item, visit location, talk to NPC, or your preferred objective type.
   - Rewards: XP, items, currency, world flags, or project-specific reward handlers.
5. Open **Tools > RPG Toolkit > Quest Editor** to inspect and validate the quest.

To connect the quest to dialogue, add dialogue commands or project-local command handling such as:

- Start quest when the player accepts the elder's request.
- Advance objective when herbs are collected.
- Complete quest when the player returns.
- Grant rewards on completion.

Use stable IDs consistently across dialogue, inventory, quest objectives, and save data.

## 12. Build a small map

The map workflow lets you move from source art to reusable map definitions.

### Create sprite sheet metadata

1. Put your source tile art under `Assets/RPG/Maps/Art/`.
2. Open **Tools > RPG Toolkit > Maps > Sprite Sheet Editor**.
3. Create or select a sprite sheet asset.
4. Define tile size, spacing, pivot, and slicing metadata.
5. Validate the generated metadata.

### Create a tileset

1. Open **Tools > RPG Toolkit > Maps > Tileset Editor**.
2. Create a new tileset definition or use **Tools > RPG Toolkit > Maps > Create Tileset Definition**.
3. Assign the sprite sheet asset.
4. Define tile entries, collision/blocker flags, terrain tags, and any tile metadata your game uses.

### Create and paint a map

1. Use **Tools > RPG Toolkit > Maps > Create Map Definition**.
2. Name the asset `VillageMap.asset`.
3. Open **Tools > RPG Toolkit > Maps > Map Editor**.
4. Select the map and tileset.
5. Paint ground, walls, decorations, blockers, zones, and placed objects.
6. Save the map asset.

### Preview runtime loading

1. Open **Tools > RPG Toolkit > Maps > Runtime Map Preview**.
2. Select `VillageMap.asset`.
3. Use preview or load actions to instantiate the map in a scene.
4. Confirm blockers, zones, transitions, and placed objects are reported correctly.

For deeper map-specific steps, read `maps-workflow.md` and the imported **Map Workflow Tutorial** sample.

## 13. Add scene transitions and world state

Scene transitions should be data-driven enough to support doors, map exits, teleporters, and cutscenes.

1. Add a door or exit GameObject to your scene.
2. Add a collider and door/trigger interaction component.
3. Configure the target scene, spawn point, or map connection payload.
4. Create named spawn points using the toolkit world/spawn components or your own scene markers.
5. Test the transition in Play Mode.

World state tracks flags and values such as:

- `elder.quest.started`
- `elder.quest.completed`
- `village.gate.open`
- `boss.defeated`

Use **Tools > RPG Toolkit > World State Debugger** during Play Mode to inspect and troubleshoot state changes.

## 14. Add combat, abilities, loot, and vendors

The toolkit supplies content definitions and lightweight runtime services for combat-adjacent systems. Your game still decides animation, input, hitboxes, and presentation.

### Damage and status effects

1. Create damage types with **Create > RPG Toolkit > Combat > Damage Type**.
2. Create status effects with **Create > RPG Toolkit > Combat > Status Effect**.
3. Use the damage system from project-local combat scripts to apply damage and modifiers.

### Abilities

1. Right-click and choose **Create > RPG Toolkit > Ability Definition**.
2. Configure ID, display name, costs, cooldowns, tags, and effect data if available.
3. Open **Tools > RPG Toolkit > Ability Editor** for focused browsing and creation.
4. Call the ability executor from your player/enemy controller or combat state machine.

### Loot tables

1. Right-click and choose **Create > RPG Toolkit > Loot Table**.
2. Add item entries, weights, quantities, and conditions.
3. Open **Tools > RPG Toolkit > Loot Table Editor** or **Advanced Loot Table Editor** to validate and simulate results.
4. Trigger loot rolls from enemy defeat, treasure chests, or quest rewards.

### Vendors

1. Right-click and choose **Create > RPG Toolkit > Vendor**.
2. Configure stock, prices, refresh rules, and any conditions.
3. Open **Tools > RPG Toolkit > Vendor Editor**.
4. Build project-local shop UI that reads the vendor definition and modifies inventory/currency.

## 15. Add crafting, factions, AI, schedules, cutscenes, and events

These systems are optional but useful once the core loop is working.

- **Crafting**: create recipes with **Create > RPG Toolkit > Crafting Recipe**, group them in a recipe database, and open **Tools > RPG Toolkit > Crafting Editor**.
- **Factions**: create faction definitions and a faction database, then open **Tools > RPG Toolkit > Faction Database Editor**.
- **Skill trees**: create skill tree assets and open **Tools > RPG Toolkit > Skill Tree Designer**.
- **AI behavior trees**: create behavior tree definitions and open **Tools > RPG Toolkit > AI > Behavior Tree Editor**.
- **NPC schedules**: create NPC schedule definitions and open **Tools > RPG Toolkit > World > NPC Schedule Editor**.
- **Events**: create RPG event assets and open **Window > RPG Toolkit > Event Editor**.
- **Cutscenes**: create cutscene definitions and open **Tools > RPG Toolkit > Cutscene Editor**.

Add these after your first playable vertical slice, not before. A small, complete loop is easier to debug than many disconnected systems.

## 16. Save and load game state

The save system is contributor-based: each subsystem can provide a payload for the save file and restore from it later.

Typical save contributors include:

- Character state.
- Inventory contents.
- Quest journal progress.
- World flags and values.
- Party roster.
- Current map/scene/spawn point.

Recommended workflow:

1. Decide which systems must persist for your first milestone.
2. Add the toolkit save service or a project-local save manager to your bootstrap scene.
3. Register built-in save contributors for inventory, character, world state, quests, and party as needed.
4. Add UI buttons or debug hotkeys for save and load.
5. Enter Play Mode, change inventory/quest/world state, save, reload, and verify restoration.
6. Open **Tools > RPG Toolkit > Save Data Debugger** to inspect the serialized payload.

Use stable IDs for all save-referenced content. Saves should store IDs and simple state, not fragile scene object references.

## 17. Create UI adapters

The package avoids forcing one RPG UI. You will usually build project-local adapters for:

- Inventory screen.
- Equipment screen.
- Dialogue panel.
- Quest journal.
- Shop window.
- Loot popup.
- Character sheet.
- Save/load menu.
- Interaction prompt.

Best practice:

1. Keep UI prefabs in `Assets/RPG/UI/`.
2. Keep adapter scripts in `Assets/Scripts/RPG/UI/`.
3. Let adapters read toolkit runtime models and invoke public toolkit APIs.
4. Keep animation, styling, sound, localization, and input rebinding in your game project.

This separation keeps the package reusable while still allowing your game to feel custom.

## 18. Validate your vertical slice

A good first vertical slice should include:

1. A player character with stats and resources.
2. One map or scene with blockers and a spawn point.
3. One pickup that adds an item to inventory.
4. One NPC with dialogue.
5. One quest started through dialogue.
6. One objective advanced by pickup, world state, or interaction.
7. One reward granted on quest completion.
8. One save/load cycle preserving inventory, quest state, and world state.

Validation checklist:

- Open **Tools > RPG Toolkit > Dashboard** and run package/workflow validation.
- Open database/editor windows and check for missing references.
- Enter Play Mode and complete the slice from a clean save.
- Save, exit Play Mode, re-enter Play Mode, load, and verify state.
- Check the Console for validation warnings and exceptions.

## 19. Recommended production workflow

Use this order for real projects:

1. **Prototype content in ScriptableObjects**: stats, resources, items, characters, dialogue, and quests.
2. **Build one scene loop**: player, NPC, pickup, quest, save/load.
3. **Add maps**: tilesets, map definitions, transitions, and runtime loading.
4. **Add UI adapters**: inventory, dialogue, quest journal, save menu.
5. **Add combat and abilities**: connect definitions to your controller, hit detection, animations, and VFX.
6. **Add economy systems**: vendors, loot, crafting, rewards.
7. **Add world depth**: factions, schedules, cutscenes, events, party systems.
8. **Harden saves**: migration, versioning, invalid-content handling, and QA fixtures.
9. **Create content packs**: split databases by region, chapter, or DLC-style module.
10. **Automate validation**: run Unity Test Runner suites and package validation before releases.

## 20. Extending the toolkit safely

Prefer extension over package edits:

- Put game-specific scripts under `Assets/Scripts`.
- Create adapter components that call toolkit APIs.
- Create custom inspectors in your project if you need game-specific authoring UI.
- Use stable IDs and tags for cross-system references.
- Avoid direct scene references in data assets when an ID, tag, spawn point, or addressable reference would be safer.

Edit package source only when you are improving the reusable toolkit itself. If you do edit package source, update tests and documentation with the change.

## 21. Troubleshooting

### Package does not appear in Package Manager

- Confirm `package.json` exists at the selected package root.
- Confirm the package name is `com.sixstringsyn.rpgtoolkit2d`.
- Reopen Unity or use **Assets > Reimport All** if Unity cached stale package metadata.

### Toolkit menus do not appear

- Wait for scripts to compile.
- Check the Console for compile errors.
- Confirm the editor assembly definition is included in the package.
- Confirm your Unity version is compatible with Unity 6 package metadata.

### Assets cannot be created from the Create menu

- Check for compile errors.
- Confirm you are right-clicking inside the Project window, not Hierarchy.
- Try creating the asset from a toolkit editor window if one exists for that content type.

### Dialogue or quests do not advance

- Confirm all IDs match exactly.
- Confirm dialogue commands are registered with the runtime context you are using.
- Confirm quest objectives reference the same item, state key, NPC, or event IDs that your gameplay code emits.

### Save/load misses data

- Confirm the subsystem has a registered save contributor.
- Confirm content IDs are stable and unique.
- Inspect the payload with **Save Data Debugger**.
- Test with a clean save slot to rule out old incompatible data.

## 22. Where to go next

After completing this tutorial, read the focused documentation pages:

- `getting-started.md` for the shortest first milestone.
- `systems.md` for runtime system details.
- `editor-tools.md` for all editor windows.
- `maps-workflow.md` for complete map authoring.
- `api.md` for namespaces and extension points.
- `extension-guide.md` for customization patterns.
- `troubleshooting.md` for common issues.

