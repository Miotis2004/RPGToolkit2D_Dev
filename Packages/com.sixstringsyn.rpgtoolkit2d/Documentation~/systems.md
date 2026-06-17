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
