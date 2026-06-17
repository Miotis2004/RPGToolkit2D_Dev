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
