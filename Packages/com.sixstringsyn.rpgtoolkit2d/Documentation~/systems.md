# RPG Toolkit 2D Systems

## Shared Core Concepts

### Stable Identifiers

Every reusable content definition derives from `RPGObject` and owns an `RPGId`. IDs are intended to be stable across renames, scene changes, and save/load boundaries.

```csharp
var id = RPGId.NewId();
var sameId = new RPGId(id.Value);
Debug.Assert(id == sameId);
```

### Definition Assets

Phase 2 includes these base definition assets:

- `CharacterDefinition`
- `ItemDefinition`
- `QuestDefinition`
- `DialogueDefinition`
- `AbilityDefinition`

Create them from **Assets > Create > RPG Toolkit > ... Definition**. These base assets intentionally contain only shared metadata for now; later phases will add system-specific fields.

### Tags and Queries

Use `RPGTag` for lightweight classification such as `element.fire`, `faction.villager`, or `rarity.legendary`.

```csharp
var tags = new[] { new RPGTag("element.fire"), new RPGTag("boss") };
var isFireBoss = RPGTagQuery.HasAll(tags, new[]
{
    new RPGTag("element.fire"),
    new RPGTag("boss")
});
```

### Definition Databases

`RPGDatabase<T>` is an in-memory index for definition collections. It can locate definitions by ID and report content validation diagnostics, including duplicate IDs.

```csharp
var database = new RPGDatabase<ItemDefinition>(itemDefinitions);
if (database.TryGet(new RPGId("item.potion"), out var potion))
{
    Debug.Log(potion.DisplayName);
}

var validation = database.Validate();
if (!validation.IsValid)
{
    foreach (var message in validation.Messages)
    {
        Debug.LogWarning(message.Message);
    }
}
```

## Character, Stats, Leveling, and Resources

Phase 3 adds deterministic runtime models for character definitions and character instances. `CharacterDefinition` remains the authored ScriptableObject, while `CharacterInstance` owns mutable runtime state such as current level, total experience, calculated stats, and resource pools.

### Stat Definitions and Stat Blocks

Create stats from **Assets > Create > RPG Toolkit > Stats > Stat Definition**. A stat definition supplies default, minimum, and maximum values. Runtime values live in `StatBlock`, which combines base values from a character stat template with modifiers.

Modifier calculation order is deterministic:

1. Start from the stat base value.
2. Apply all active additive modifiers ordered by their `order` value.
3. Apply all active multiplicative modifiers ordered by their `order` value.
4. Apply active override modifiers ordered by their `order` value.
5. Clamp the final value to the stat definition's minimum and maximum.

Modifiers may be temporary, allowing systems such as buffs or equipment previews to remove them as a group. Conditional modifiers can require a context tag, such as `state.enraged`, before they contribute to a calculated stat.

### Resource Pools

Create resources from **Assets > Create > RPG Toolkit > Stats > Resource Definition**. A resource can use a stat as its maximum value, such as a Health resource using a Max Health stat, or fall back to a fixed maximum. Runtime `ResourcePool` instances clamp consumption, healing, and regeneration between zero and the current maximum.

### Character Definitions and Instances

A character definition now includes starting level, portrait, optional prefab, experience curve, stat template, and resource entries. Use the same definition type for player characters, enemies, NPCs, and companions by combining tags with project-specific conventions.

```csharp
var heroInstance = new CharacterInstance(heroDefinition);
heroInstance.LevelChanged += (character, before, after) => Debug.Log($"Level {before} -> {after}");
heroInstance.StatChanged += (character, stat, before, after) => Debug.Log($"{stat.DisplayName}: {after}");
```

Example authoring patterns:

- **Player character:** assign a portrait, player prefab, starting resources, and tags such as `actor.player`.
- **Enemy:** assign combat stats, a scene prefab, an XP curve if enemies can level, and tags such as `faction.monster`.
- **Companion:** assign party-facing tags such as `actor.companion`, a portrait, and the same resource/stat templates used by players.

### XP Curves and Level-Up Rules

`ExperienceCurveDefinition` stores total XP thresholds per level. `CharacterInstance.AddExperience` increases total XP and raises `LevelChanged` when the curve maps the new total to a different level.

### Runtime Events

`CharacterInstance` exposes events for level changes, calculated stat changes, and resource changes. These events are intended for UI adapters, save systems, combat systems, and custom game rules.

### Troubleshooting

- If a calculated stat returns zero, confirm the `CharacterDefinition` stat template references a valid `StatDefinition`.
- If a resource maximum is not expected, confirm the resource's maximum stat is present in the character stat template.
- If characters never level up, confirm the character references an `ExperienceCurveDefinition` with increasing total XP thresholds.
- If a conditional modifier does not apply, confirm the stat block context tags contain the required tag.

## Items, Inventory, Equipment, and Pickups

Phase 4 adds the first end-to-end playable item workflow: authored item definitions can be collected from scene pickups, placed in runtime inventories, equipped into validated slots, used through extension actions, and converted to save data.

### Item Definitions

Create items from **Assets > Create > RPG Toolkit > Item Definition**. `ItemDefinition` now includes:

- Display name, description, and tags inherited from `RPGObject`.
- Icon for inventory UI adapters.
- Maximum stack size. Values greater than one make an item stackable.
- Rarity: common, uncommon, rare, epic, or legendary.
- Item type: generic, consumable, weapon, armor, or quest item.
- Optional allowed equipment slot IDs.
- Optional `ItemUseAction` extension asset.

Recommended categories:

- **Consumables:** stackable items with an `ItemUseAction` that applies healing, buffs, keys, or custom effects.
- **Weapons and armor:** quantity one items with one or more allowed equipment slot IDs.
- **Quest items:** usually non-stackable or carefully stacked items that quests can query by item ID.
- **Generic items:** crafting ingredients, valuables, or miscellaneous loot.

### Item Instances

`ItemInstance` represents mutable runtime state for a definition. It stores quantity, durability, generated stat modifiers, and string-based custom state for game-specific data. Keep authored fields on `ItemDefinition` and per-copy fields on `ItemInstance`.

### Inventory Containers

`InventoryContainer` is a deterministic runtime model with fixed capacity slots. It supports:

- Add with automatic stack filling and overflow reporting.
- Remove by item definition and quantity.
- Move, swap, merge, and split operations.
- Count and contains queries.
- `Changed` and `SlotChanged` events for UI adapters.

```csharp
var inventory = new InventoryContainer(20);
var added = inventory.Add(potionDefinition, 3);
if (inventory.Contains(potionDefinition, 1))
{
    inventory.Remove(potionDefinition, 1);
}
```

### Equipment

`EquipmentSlotDefinition` defines slot IDs such as `main_hand`, `off_hand`, `head`, or `body`. `EquipmentContainer` validates an item before equipping it: the item must have quantity one and its definition must either allow the target slot ID or leave the allowed slot list empty for project-defined universal equipment.

### Item Use Extension Points

Create custom use behavior by deriving from `ItemUseAction` and assigning the action asset to an item definition. Use `CanUse` for validation and `Use` to mutate game state. The `user` and `context` arguments are deliberately generic so projects can pass character instances, MonoBehaviours, combat contexts, or custom service objects without the toolkit forcing a game architecture.

### Pickups

Add `ItemPickup` to a scene GameObject, assign an `ItemDefinition`, and set the quantity. Call `Collect(InventoryContainer)` or `Collect(InventoryComponent)` from your interaction code. The pickup adds as many items as possible and reduces its remaining quantity; by default it destroys its GameObject when fully collected.

### Save and Load

`InventorySaveData.FromInventory` captures item IDs, quantities, and durability. `ToInventory` rebuilds an inventory by resolving saved `RPGId` values back to `ItemDefinition` assets. Projects can plug in `RPGDatabase<ItemDefinition>` or another content service as the resolver.

## Save and Load System

Phase 5 introduces a JSON save/load foundation designed around save slots and per-system contributors. `SaveGameService` captures a `GameSaveData` container with metadata and each registered system payload. `SaveSlotService` writes and reads that JSON from named slot files under `Application.persistentDataPath/RPGToolkit2D/Saves` by default.

### Save Metadata

Each save includes high-level metadata for slot selection UI and debugging:

- save format version
- package version
- created and updated UTC timestamps
- playtime seconds
- active scene name

Projects should treat save JSON as toolkit-owned and use the save APIs instead of hand-editing fields; migrate older data with `ISaveMigration` implementations.

### Save Contributors

Systems participate in saves by implementing `ISaveContributor`. Contributors expose a stable `SystemId`, return JSON from `CaptureJson`, and restore themselves from `RestoreJson`.

```csharp
var saveService = new SaveGameService();
saveService.Register(new InventorySaveContributor(
    () => playerInventory,
    restored => playerInventory = restored,
    id => itemDatabase.TryGet(id, out var item) ? item : null,
    playerInventory.Capacity));

var slots = new SaveSlotService(saveService);
slots.Save("slot1", saveService.Capture(playtimeSeconds, "Town"));
```

The package includes contributors and data models for inventory and character state. Quest, world, and scene state save data classes are placeholders so projects can begin wiring flags, completed quests, visited scenes, and consumed scene objects without waiting for later gameplay phases.

### Loading and Failure Handling

`SaveSlotService.Load` returns a `SaveResult` instead of throwing for normal failure cases. Missing slots, unreadable files, and corrupted JSON fail gracefully so games can display a recovery message or ignore the slot.

### Migration Guidance

`SaveGameService` checks the save version before restore. Register `ISaveMigration` implementations to upgrade older save data to `SaveConstants.CurrentSaveVersion`. Until a migration exists for a version mismatch, restore fails safely instead of applying unknown data.

### Save Data Debugger

Open **Tools > RPG Toolkit > Save Data Debugger** to inspect available save slots, their paths, scene names, versions, and update timestamps. The debugger is intentionally read-only in Phase 5.

## Interactions and Scene Transitions

Phase 6 standardizes player-facing interactions around `IInteractable`. NPCs, pickups, doors, and trigger-style objects expose a label, priority, availability check, and interaction method so projects can route keyboard, gamepad, touch, or custom input through one flow.

### Interaction Detection

`InteractionDetector` can maintain candidates from 2D trigger callbacks, scan an overlap circle, or raycast in a facing direction. It selects the highest-priority interactable that currently passes `CanInteract` and notifies optional prompt UI through `IInteractionPrompt`.

### Built-in Interactables

- `NPCInteraction` references an optional `CharacterDefinition` and `DialogueDefinition`, then raises an event for dialogue or quest systems.
- `PickupInteraction` adapts an `ItemPickup` so interacting with it collects into the interactor's `InventoryComponent`.
- `DoorInteraction` creates a `SceneTransitionRequest` for a target scene and spawn point.
- `TriggerInteraction` is a generic event-based interactable for switches, cutscenes, and project-specific logic.

### Scene Transitions and Spawn Points

`SceneTransitionRequest` carries the target scene name, optional spawn point ID, and whether callers want to save before moving scenes. `SceneTransitionService` raises transition requests for project loading screens, fade controllers, save hooks, or direct scene loading adapters. Add `SpawnPoint` to destination scene objects and assign stable IDs such as `default`, `north_gate`, or `dungeon_exit`.

Validate interactables before shipping: labels should be present for prompt UI, and doors must have a non-empty target scene name.

## Dialogue System and Graph Editor

Phase 7 adds data-driven branching dialogue. `DialogueDefinition` assets contain serializable nodes, choices, conditions, and commands that can be authored directly or through **Tools > RPG Toolkit > Dialogue Graph Editor**.

### Node Types

- `Entry` nodes are valid conversation start points.
- `Line` nodes display speaker text and may offer choices or continue to a next node.
- `Exit` nodes end the active `DialogueRunner` session.

### Choices, Conditions, and Commands

Choices link to target nodes and can include conditions. The built-in `DialogueConditionEvaluator` reads string values from `IDialogueContext` and supports existence, equality, inequality, and numeric or string comparisons. Commands are lightweight name/argument payloads raised by `DialogueRunner.CommandExecuted` so games can grant items, set quest flags, play animations, or run project-specific scripting.

```csharp
var context = new DictionaryDialogueContext();
context.Set("has_key", "true");
runner.CommandExecuted += command => Debug.Log(command.Name);
runner.Start(dialogueDefinition, context);
```

### Runtime Presentation

`DialogueRunner` owns deterministic traversal and exposes `CurrentNode`, `AvailableChoices`, `Continue`, and `SelectChoice`. Implement `IDialoguePresenter` on a UI adapter to display lines and hide the dialogue panel when the runner ends. `NPCDialogueAdapter` listens to `NPCInteraction.Interacted` and starts the assigned `DialogueDefinition`, allowing scene NPCs to launch conversations without coupling the toolkit to a specific UI prefab.

### Authoring Workflow

1. Create or select a `DialogueDefinition` asset.
2. Open **Tools > RPG Toolkit > Dialogue Graph Editor**.
3. Add line and exit nodes, assign speaker/text content, and link nodes with `NextNodeId` or choices.
4. Use the editor validation button, or call `ValidateGraph`, to find empty graphs and missing links.
5. Assign the dialogue asset to an `NPCInteraction` and add an `NPCDialogueAdapter` to start it from interaction.

## Quest System

`QuestDefinition` assets describe prerequisites, objectives, rewards, and whether a project should turn the quest in automatically. Runtime quest flow is managed by `QuestTracker`, which starts quests once `QuestCondition` entries pass, advances objectives from gameplay events, saves `QuestJournalSaveData`, and restores quest instances from known definitions.

Objective types include collect item, talk to NPC, reach location, kill target, custom event, and custom script. Built-in objectives match either an `ItemDefinition` or a target string, while custom script objectives are intentionally data-only extension points for game-specific code.

Rewards support items, experience, currency, and custom actions through `IQuestRewardReceiver`. Projects can use `QuestRewardReceiver` for inventory, character XP, and simple currency storage, or implement the interface to connect rewards to another economy, party, or analytics system.

Dialogue can drive quest progress by binding a `QuestTracker` to a `DialogueRunner` and emitting a `DialogueCommand` named `quest_event`; the command argument is matched against custom event objective target IDs. The Quest Editor is available from **Tools > RPG Toolkit > Quest Editor** for creating, inspecting, and validating quest assets.


## Phase 9 Abilities, Combat, Status Effects, Loot, and Vendors

Phase 9 adds presentation-agnostic combat primitives. `DamageCalculator` accepts a `DamageRequest` with damage type, target resource, attack and defense stats, critical-hit values, and an optional mitigation hook so projects can layer armor formulas, elemental resistance, grid cover, or turn-based modifiers without replacing the core model. `IHitDetector`, `ICombatTargetValidator`, and `ICombatAIHook` are adapter contracts for action, tactical, or menu-driven combat.

`AbilityDefinition` assets define resource costs, cooldowns, range, targeting mode, effects, and tags. `AbilityExecutor` validates cost/cooldown/targets, consumes resources, applies effects, and starts cooldowns. `MeleeAttackAdapter` and `ProjectileAttackAdapter` show how direct attacks and hit-detected projectiles can share the same executor.

`StatusEffectDefinition` and `StatusEffectController` support duration, stacking modes, tick intervals, periodic damage/healing effects, and removal rules. Use these for examples such as poison by configuring a periodic effect against the target health resource.

`LootTableDefinition` contains weighted item drops with quantity ranges; `LootRoller.Roll` accepts a deterministic `System.Random` seed for repeatable tests or roguelike runs. `VendorDefinition` and `VendorShop` provide stock, buy/sell pricing, inventory integration, and restock hooks for shopkeepers.

Authoring menu entries are available under **Tools > RPG Toolkit** for combat tuning, abilities, loot tables, and vendors.

## Phase 10 World State, Party, Companions, NPCs, and Crafting

Phase 10 adds persistent world state, party roster management, companion recruitment state, NPC definitions, and inventory-backed crafting. Use dot-separated world state keys such as `quest.mira_helped`, `door.forest_gate.unlocked`, and `counter.wolf_kills` so saves remain readable and debugger-friendly. `WorldStateCondition` can gate dialogue choices, quest objectives, vendors, scene transitions, or custom adapters by checking existence, equality, and numeric thresholds.

`PartyRoster` tracks all recruited members, the active party subset, companion relationship values, and recruitment hooks. A companion can be recruited only after its supplied world-state conditions pass, then the roster can be captured through `PartySaveContributor` alongside `WorldStateSaveContributor`.

`NPCDefinition` groups a character definition with optional dialogue, vendor data, and recruitment metadata. Scene objects can attach `NPCComponent` to expose this definition while existing interaction and dialogue adapters continue to handle presentation.

`CraftingRecipeDefinition` defines ingredients, outputs, an optional station id such as `alchemy`, and an optional currency cost. `CraftingSystem.Validate` checks station, currency, ingredients, and outputs before `Craft` removes ingredients and adds outputs to the same `InventoryContainer`; UI can implement `ICraftingPresenter` to display validation results and crafted feedback.

Examples:

- **Recruited companion:** set `quest.mira_helped=true`, evaluate that key in a `WorldStateCondition`, then call `PartyRoster.Recruit(mira, worldState, conditions)`.
- **Locked door flag:** store `door.forest_gate.unlocked=false` or clear/set the key when a lever changes state, then gate door interaction with `WorldStateCondition`.
- **Crafting station:** create a recipe with `StationId` set to `alchemy`; it will validate only when the active station passes the same id.

Save/load behavior is contributor-based. Register `WorldStateSaveContributor` and `PartySaveContributor` with `SaveGameService` so these systems write independent JSON payloads into the normal game save.
