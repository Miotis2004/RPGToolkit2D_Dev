using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Maps
{
    public enum RPGMapLayerKind { Ground, Decoration, Collision, Overhead, Trigger }
    public enum RPGMapZoneKind { Region, Encounter, Weather, Lighting, Spawn }
    public enum RPGMapMetadataScope { Tile, Layer, Collision, Zone, Object, Connection }
    public enum RPGMapObjectCategory { NPC, Monster, EncounterSpawn, Item, Chest, Door, ExitTransition, SavePoint, Decoration, Custom }
    public enum RPGMapDirection { None, North, East, South, West }
    public enum RPGMapTransitionKind { Instant, Fade, Door, Stairs, Custom }
    public enum RPGMapStorageProfile { Auto, Sparse, Dense, Chunked }

    [Serializable]
    public sealed class RPGMapPerformanceSettings
    {
        public RPGMapStorageProfile storageProfile = RPGMapStorageProfile.Auto;
        public Vector2Int chunkSize = new Vector2Int(32, 32);
        public bool cacheRuntimeQueries = true;
        public int denseLayerThresholdPercent = 60;
    }

    [Serializable]
    public sealed class RPGMapMetadataEntry
    {
        public string key;
        public string value;
    }

    [Serializable]
    public sealed class RPGMapTile
    {
        public Vector2Int position;
        public string tileId;
        public int rotationDegrees;
        public bool flipX;
        public bool flipY;
        public bool overrideCollision;
        public bool blocksMovementOverride;
        public List<RPGMapMetadataEntry> overrideMetadata = new List<RPGMapMetadataEntry>();
    }

    [Serializable]
    public sealed class RPGMapLayer
    {
        public string layerId = "Layer";
        public string displayName = "Layer";
        public int renderOrder;
        public bool visible = true;
        public bool locked;
        [Range(0f, 1f)] public float opacity = 1f;
        public RPGMapLayerKind kind;
        public string unityTilemapTargetName;
        public List<RPGMapTile> tiles = new List<RPGMapTile>();
    }

    [Serializable]
    public sealed class RPGMapObjectPlacement
    {
        public string objectId;
        public string displayName;
        public GameObject prefab;
        public string addressableKey;
        public Vector2Int gridPosition;
        public Vector3 worldOffset;
        public Vector3 rotationEuler;
        public Vector3 scale = Vector3.one;
        public bool snapToGrid = true;
        public string spawnConditionKey;
        public string persistentStateKey;
        public RPGMapObjectCategory category = RPGMapObjectCategory.Custom;
        public RPGMapDirection facing = RPGMapDirection.South;
        public bool blocksMovement;
        public List<RPGMapMetadataEntry> metadata = new List<RPGMapMetadataEntry>();
    }

    [Serializable]
    public sealed class RPGMapObjectSpawnDescriptor
    {
        public string objectId;
        public string displayName;
        public RPGMapObjectCategory category;
        public GameObject prefab;
        public string addressableKey;
        public Vector2Int gridPosition;
        public Vector3 worldPosition;
        public Quaternion rotation;
        public Vector3 scale;
        public string persistentStateKey;
        public IReadOnlyList<RPGMapMetadataEntry> metadata;
    }

    public interface IRPGMapPrefabResolver
    {
        GameObject ResolvePrefab(RPGMapObjectPlacement placement);
    }

    public interface IRPGMapObjectStateResolver
    {
        bool ShouldSpawn(RPGMapObjectPlacement placement);
    }

    public interface IRPGMapTransitionHandler
    {
        void HandleTransition(RPGMapDefinition sourceMap, RPGMapExit exit, RPGMapDefinition targetMap, RPGMapEntrance targetEntrance);
    }

    public sealed class RPGMapConnectionResolution
    {
        public RPGMapExit Exit { get; set; }
        public RPGMapDefinition TargetMap { get; set; }
        public RPGMapEntrance TargetEntrance { get; set; }
        public bool IsResolved => Exit != null && TargetMap != null && (string.IsNullOrWhiteSpace(Exit.targetEntranceId) || TargetEntrance != null);
    }

    public static class RPGMapObjectSpawner
    {
        public static IEnumerable<RPGMapObjectSpawnDescriptor> BuildSpawnDescriptors(RPGMapDefinition map, IRPGMapPrefabResolver prefabResolver = null, IRPGMapObjectStateResolver stateResolver = null)
        {
            if (map == null) yield break;
            foreach (var placement in map.Objects)
            {
                if (placement == null || stateResolver?.ShouldSpawn(placement) == false) continue;
                yield return new RPGMapObjectSpawnDescriptor
                {
                    objectId = placement.objectId,
                    displayName = string.IsNullOrWhiteSpace(placement.displayName) ? placement.objectId : placement.displayName,
                    category = placement.category,
                    prefab = prefabResolver?.ResolvePrefab(placement) ?? placement.prefab,
                    addressableKey = placement.addressableKey,
                    gridPosition = placement.gridPosition,
                    worldPosition = new Vector3(placement.gridPosition.x, placement.gridPosition.y, 0f) + placement.worldOffset,
                    rotation = Quaternion.Euler(placement.rotationEuler),
                    scale = placement.scale == Vector3.zero ? Vector3.one : placement.scale,
                    persistentStateKey = placement.persistentStateKey,
                    metadata = placement.metadata ?? new List<RPGMapMetadataEntry>()
                };
            }
        }
    }

    [Serializable]
    public sealed class RPGMapZone
    {
        public string zoneId;
        public string displayName;
        public RPGMapZoneKind kind;
        public RectInt bounds;
        public string payloadId;
        public int priority;
        public string payloadType;
        public List<string> tags = new List<string>();
        public List<string> conditionKeys = new List<string>();
    }

    [Serializable]
    public sealed class RPGMapEntrance
    {
        public string entranceId;
        public Vector2Int position;
        public RPGMapDirection facing = RPGMapDirection.South;
    }

    [Serializable]
    public sealed class RPGMapExit
    {
        public string exitId;
        public Vector2Int position;
        public RPGMapDefinition targetMap;
        public string targetMapId;
        public string targetEntranceId;
        public RPGMapDirection facing = RPGMapDirection.South;
        public RPGMapTransitionKind transitionKind = RPGMapTransitionKind.Fade;
    }

    [Serializable]
    public sealed class RPGMapConnection
    {
        public string connectionId;
        public RPGMapDefinition targetMap;
        public string targetMapId;
        public string targetEntranceId;
        public RPGMapDirection facing = RPGMapDirection.South;
        public RPGMapTransitionKind transitionKind = RPGMapTransitionKind.Fade;
    }

    [Serializable]
    public sealed class RPGMapEffectiveMetadataEntry
    {
        public RPGMapMetadataScope scope;
        public string ownerId;
        public string key;
        public string value;
        public int priority;
    }

    [Serializable]
    public sealed class RPGMapCellMetadata
    {
        public Vector2Int position;
        public bool blocked;
        public List<RPGMapTile> tiles = new List<RPGMapTile>();
        public List<RPGMapZone> zones = new List<RPGMapZone>();
        public List<RPGMapObjectPlacement> objects = new List<RPGMapObjectPlacement>();
        public List<RPGMapEntrance> entrances = new List<RPGMapEntrance>();
        public List<RPGMapExit> exits = new List<RPGMapExit>();
        public List<RPGMapEffectiveMetadataEntry> metadata = new List<RPGMapEffectiveMetadataEntry>();
    }

    [CreateAssetMenu(fileName = "RPGMapDefinition", menuName = "RPG Toolkit/Maps/Map")]
    public sealed class RPGMapDefinition : RPGObject, ISerializationCallbackReceiver
    {
        [SerializeField] private Vector2Int _size = new Vector2Int(32, 32);
        [SerializeField] private RPGTilesetDefinition _tileset;
        [SerializeField] private List<RPGMapLayer> _layers = new List<RPGMapLayer>();
        [SerializeField] private List<RPGMapZone> _zones = new List<RPGMapZone>();
        [SerializeField] private List<RPGMapConnection> _connections = new List<RPGMapConnection>();
        [SerializeField] private List<RPGMapObjectPlacement> _objects = new List<RPGMapObjectPlacement>();
        [SerializeField] private List<RPGMapEntrance> _entrances = new List<RPGMapEntrance>();
        [SerializeField] private List<RPGMapExit> _exits = new List<RPGMapExit>();
        [SerializeField] private RPGMapPerformanceSettings _performanceSettings = new RPGMapPerformanceSettings();

        [NonSerialized] private bool _queryCacheDirty = true;
        [NonSerialized] private readonly Dictionary<string, Dictionary<Vector2Int, RPGMapTile>> _tilesByLayer = new Dictionary<string, Dictionary<Vector2Int, RPGMapTile>>(StringComparer.OrdinalIgnoreCase);
        [NonSerialized] private readonly Dictionary<Vector2Int, List<RPGMapTile>> _tilesByPosition = new Dictionary<Vector2Int, List<RPGMapTile>>();
        [NonSerialized] private readonly Dictionary<Vector2Int, List<RPGMapZone>> _zonesByPosition = new Dictionary<Vector2Int, List<RPGMapZone>>();
        [NonSerialized] private readonly Dictionary<Vector2Int, List<RPGMapObjectPlacement>> _objectsByPosition = new Dictionary<Vector2Int, List<RPGMapObjectPlacement>>();
        [NonSerialized] private readonly HashSet<Vector2Int> _blockedPositions = new HashSet<Vector2Int>();

        public Vector2Int Size => _size;
        public RPGTilesetDefinition Tileset => _tileset;
        public IReadOnlyList<RPGMapLayer> Layers => _layers;
        public IReadOnlyList<RPGMapZone> Zones => _zones;
        public IReadOnlyList<RPGMapConnection> Connections => _connections;
        public IReadOnlyList<RPGMapObjectPlacement> Objects => _objects;
        public IReadOnlyList<RPGMapEntrance> Entrances => _entrances;
        public IReadOnlyList<RPGMapExit> Exits => _exits;
        public RPGMapPerformanceSettings PerformanceSettings => _performanceSettings;

        public void OnBeforeSerialize() => MigrateExpandedMapData();
        public void OnAfterDeserialize() { MigrateExpandedMapData(); MarkRuntimeQueryCacheDirty(); }

        public void MigrateExpandedMapData()
        {
            if (_layers == null) _layers = new List<RPGMapLayer>();
            if (_zones == null) _zones = new List<RPGMapZone>();
            if (_connections == null) _connections = new List<RPGMapConnection>();
            if (_objects == null) _objects = new List<RPGMapObjectPlacement>();
            if (_entrances == null) _entrances = new List<RPGMapEntrance>();
            if (_exits == null) _exits = new List<RPGMapExit>();
            if (_performanceSettings == null) _performanceSettings = new RPGMapPerformanceSettings();
            if (_performanceSettings.chunkSize.x <= 0 || _performanceSettings.chunkSize.y <= 0) _performanceSettings.chunkSize = new Vector2Int(32, 32);
            for (var i = 0; i < _layers.Count; i++)
            {
                var layer = _layers[i];
                if (layer == null) continue;
                if (string.IsNullOrWhiteSpace(layer.layerId)) layer.layerId = $"Layer_{i + 1}";
                if (string.IsNullOrWhiteSpace(layer.displayName)) layer.displayName = layer.layerId;
                if (layer.opacity < 0f) layer.opacity = 1f;
                if (layer.tiles == null) layer.tiles = new List<RPGMapTile>();
                foreach (var tile in layer.tiles)
                    if (tile != null && tile.overrideMetadata == null) tile.overrideMetadata = new List<RPGMapMetadataEntry>();
            }
            foreach (var placement in _objects)
            {
                if (placement == null) continue;
                if (string.IsNullOrWhiteSpace(placement.objectId)) placement.objectId = GenerateUniqueObjectId(placement.category.ToString());
                if (placement.metadata == null) placement.metadata = new List<RPGMapMetadataEntry>();
                if (placement.scale == Vector3.zero) placement.scale = Vector3.one;
            }
        }

        public void Configure(Vector2Int size, RPGTilesetDefinition tileset = null)
        {
            _size = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
            _tileset = tileset;
            MigrateExpandedMapData();
            MarkRuntimeQueryCacheDirty();
        }

        public void SetTileset(RPGTilesetDefinition tileset) { _tileset = tileset; MarkRuntimeQueryCacheDirty(); }

        public RPGMapLayer AddLayer(string displayName = "Layer", RPGMapLayerKind kind = RPGMapLayerKind.Ground)
        {
            MigrateExpandedMapData();
            var baseName = string.IsNullOrWhiteSpace(displayName) ? "Layer" : displayName.Trim();
            var layer = new RPGMapLayer
            {
                layerId = GenerateUniqueLayerId(baseName),
                displayName = baseName,
                kind = kind,
                renderOrder = _layers.Count == 0 ? 0 : _layers.Max(existing => existing?.renderOrder ?? 0) + 1,
                visible = true,
                opacity = 1f,
                tiles = new List<RPGMapTile>()
            };
            _layers.Add(layer);
            MarkRuntimeQueryCacheDirty();
            return layer;
        }

        public bool RemoveLayer(string layerId)
        {
            var layer = FindLayer(layerId);
            var removed = layer != null && _layers.Remove(layer);
            if (removed) MarkRuntimeQueryCacheDirty();
            return removed;
        }

        public RPGMapLayer DuplicateLayer(string layerId)
        {
            var source = FindLayer(layerId);
            if (source == null) return null;
            var duplicate = new RPGMapLayer
            {
                layerId = GenerateUniqueLayerId($"{source.layerId}_Copy"),
                displayName = $"{source.displayName} Copy",
                renderOrder = _layers.Count == 0 ? 0 : _layers.Max(existing => existing?.renderOrder ?? 0) + 1,
                visible = source.visible,
                locked = source.locked,
                opacity = source.opacity,
                kind = source.kind,
                unityTilemapTargetName = source.unityTilemapTargetName,
                tiles = source.tiles?.Select(CloneTile).ToList() ?? new List<RPGMapTile>()
            };
            _layers.Add(duplicate);
            MarkRuntimeQueryCacheDirty();
            return duplicate;
        }

        public void MoveLayer(string layerId, int newIndex)
        {
            var layer = FindLayer(layerId);
            if (layer == null) return;
            _layers.Remove(layer);
            _layers.Insert(Mathf.Clamp(newIndex, 0, _layers.Count), layer);
            for (var i = 0; i < _layers.Count; i++)
                if (_layers[i] != null) _layers[i].renderOrder = i;
            MarkRuntimeQueryCacheDirty();
        }

        public bool PaintTile(string layerId, Vector2Int position, string tileId, int rotationDegrees = 0, bool flipX = false, bool flipY = false)
        {
            if (!IsInBounds(position) || string.IsNullOrWhiteSpace(tileId)) return false;
            var layer = FindLayer(layerId);
            if (layer == null || layer.locked) return false;
            if (layer.tiles == null) layer.tiles = new List<RPGMapTile>();
            layer.tiles.RemoveAll(tile => tile != null && tile.position == position);
            layer.tiles.Add(new RPGMapTile { position = position, tileId = tileId, rotationDegrees = rotationDegrees, flipX = flipX, flipY = flipY });
            MarkRuntimeQueryCacheDirty();
            return true;
        }

        public bool EraseTile(string layerId, Vector2Int position)
        {
            var layer = FindLayer(layerId);
            if (layer?.tiles == null || layer.locked) return false;
            var removed = layer.tiles.RemoveAll(tile => tile != null && tile.position == position) > 0;
            if (removed) MarkRuntimeQueryCacheDirty();
            return removed;
        }

        public int FillRectangle(string layerId, RectInt bounds, string tileId)
        {
            var changed = 0;
            foreach (var position in EnumerateClamped(bounds))
                if (PaintTile(layerId, position, tileId)) changed++;
            return changed;
        }

        public int FloodFill(string layerId, Vector2Int origin, string tileId)
        {
            var layer = FindLayer(layerId);
            if (layer == null || layer.locked || !IsInBounds(origin) || string.IsNullOrWhiteSpace(tileId)) return 0;
            var targetTileId = GetLayerTile(layerId, origin)?.tileId;
            if (string.Equals(targetTileId, tileId, StringComparison.OrdinalIgnoreCase)) return 0;
            var changed = 0;
            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            queue.Enqueue(origin);
            while (queue.Count > 0)
            {
                var position = queue.Dequeue();
                if (!visited.Add(position) || !IsInBounds(position)) continue;
                var currentTileId = GetLayerTile(layerId, position)?.tileId;
                if (!string.Equals(currentTileId, targetTileId, StringComparison.OrdinalIgnoreCase)) continue;
                if (PaintTile(layerId, position, tileId)) changed++;
                queue.Enqueue(new Vector2Int(position.x + 1, position.y));
                queue.Enqueue(new Vector2Int(position.x - 1, position.y));
                queue.Enqueue(new Vector2Int(position.x, position.y + 1));
                queue.Enqueue(new Vector2Int(position.x, position.y - 1));
            }
            return changed;
        }

        public int ReplaceTile(string layerId, string oldTileId, string newTileId)
        {
            var layer = FindLayer(layerId);
            if (layer?.tiles == null || layer.locked || string.IsNullOrWhiteSpace(newTileId)) return 0;
            var changed = 0;
            foreach (var tile in layer.tiles)
            {
                if (tile == null || !string.Equals(tile.tileId, oldTileId, StringComparison.OrdinalIgnoreCase)) continue;
                tile.tileId = newTileId;
                changed++;
            }
            if (changed > 0) MarkRuntimeQueryCacheDirty();
            return changed;
        }

        public int PaintStamp(string layerId, Vector2Int anchor, IEnumerable<RPGMapTile> stampTiles)
        {
            var changed = 0;
            if (stampTiles == null) return changed;
            foreach (var stampTile in stampTiles)
            {
                if (stampTile == null) continue;
                if (PaintTile(layerId, anchor + stampTile.position, stampTile.tileId, stampTile.rotationDegrees, stampTile.flipX, stampTile.flipY)) changed++;
            }
            return changed;
        }

        public int PaintStamp(string layerId, RPGMapStampDefinition stamp, Vector2Int anchor) => stamp == null ? 0 : PaintStamp(layerId, Vector2Int.zero, stamp.BuildTiles(anchor));


        public RPGMapObjectPlacement PlaceObject(string displayName, RPGMapObjectCategory category, Vector2Int gridPosition, GameObject prefab = null, bool snapToGrid = true)
        {
            if (!IsInBounds(gridPosition)) return null;
            MigrateExpandedMapData();
            var baseName = string.IsNullOrWhiteSpace(displayName) ? category.ToString() : displayName.Trim();
            var placement = new RPGMapObjectPlacement
            {
                objectId = GenerateUniqueObjectId(baseName),
                displayName = baseName,
                category = category,
                gridPosition = gridPosition,
                prefab = prefab,
                snapToGrid = snapToGrid,
                scale = Vector3.one
            };
            _objects.Add(placement);
            MarkRuntimeQueryCacheDirty();
            return placement;
        }

        public bool MoveObject(string objectId, Vector2Int gridPosition, Vector3? worldOffset = null)
        {
            var placement = FindObject(objectId);
            if (placement == null || !IsInBounds(gridPosition)) return false;
            placement.gridPosition = gridPosition;
            if (worldOffset.HasValue) placement.worldOffset = worldOffset.Value;
            MarkRuntimeQueryCacheDirty();
            return true;
        }

        public RPGMapObjectPlacement DuplicateObject(string objectId, Vector2Int? gridPosition = null)
        {
            var source = FindObject(objectId);
            var targetPosition = gridPosition ?? source?.gridPosition ?? Vector2Int.zero;
            if (source == null || !IsInBounds(targetPosition)) return null;
            var duplicate = CloneObject(source);
            duplicate.objectId = GenerateUniqueObjectId($"{source.objectId}_Copy");
            duplicate.displayName = string.IsNullOrWhiteSpace(source.displayName) ? $"{source.objectId} Copy" : $"{source.displayName} Copy";
            duplicate.gridPosition = targetPosition;
            _objects.Add(duplicate);
            MarkRuntimeQueryCacheDirty();
            return duplicate;
        }

        public bool RotateObject(string objectId, Vector3 rotationEuler)
        {
            var placement = FindObject(objectId);
            if (placement == null) return false;
            placement.rotationEuler = rotationEuler;
            return true;
        }

        public bool RemoveObject(string objectId)
        {
            var placement = FindObject(objectId);
            var removed = placement != null && _objects.Remove(placement);
            if (removed) MarkRuntimeQueryCacheDirty();
            return removed;
        }

        public RPGMapEntrance AddEntrance(string entranceId, Vector2Int position, RPGMapDirection facing = RPGMapDirection.South)
        {
            if (!IsInBounds(position)) return null;
            MigrateExpandedMapData();
            var entrance = new RPGMapEntrance { entranceId = GenerateUniqueEntranceId(entranceId), position = position, facing = facing };
            _entrances.Add(entrance);
            MarkRuntimeQueryCacheDirty();
            return entrance;
        }

        public RPGMapExit AddExit(string exitId, Vector2Int position, RPGMapDefinition targetMap, string targetEntranceId, RPGMapTransitionKind transitionKind = RPGMapTransitionKind.Fade)
        {
            if (!IsInBounds(position)) return null;
            MigrateExpandedMapData();
            var exit = new RPGMapExit { exitId = GenerateUniqueExitId(exitId), position = position, targetMap = targetMap, targetMapId = targetMap?.Id.ToString(), targetEntranceId = targetEntranceId, transitionKind = transitionKind };
            _exits.Add(exit);
            MarkRuntimeQueryCacheDirty();
            return exit;
        }

        public RPGMapConnectionResolution ResolveExitConnection(string exitId)
        {
            var exit = ResolveExit(exitId);
            var targetMap = exit?.targetMap;
            return new RPGMapConnectionResolution { Exit = exit, TargetMap = targetMap, TargetEntrance = targetMap?.ResolveEntrance(exit?.targetEntranceId) };
        }

        public RPGMapZone AddZone(string displayName, RPGMapZoneKind kind, RectInt bounds, string payloadId = null)
        {
            MigrateExpandedMapData();
            var baseName = string.IsNullOrWhiteSpace(displayName) ? kind.ToString() : displayName.Trim();
            var zone = new RPGMapZone
            {
                zoneId = GenerateUniqueZoneId(baseName),
                displayName = baseName,
                kind = kind,
                bounds = ClampRect(bounds.width <= 0 || bounds.height <= 0 ? new RectInt(0, 0, 1, 1) : bounds),
                payloadId = payloadId,
                priority = _zones.Count == 0 ? 0 : _zones.Max(existing => existing?.priority ?? 0) + 1
            };
            _zones.Add(zone);
            MarkRuntimeQueryCacheDirty();
            return zone;
        }

        public bool RemoveZone(string zoneId)
        {
            var zone = FindZone(zoneId);
            var removed = zone != null && _zones.Remove(zone);
            if (removed) MarkRuntimeQueryCacheDirty();
            return removed;
        }

        public RPGMapZone FindZone(string zoneId) => string.IsNullOrWhiteSpace(zoneId) ? null : _zones.FirstOrDefault(zone => zone != null && string.Equals(zone.zoneId, zoneId, StringComparison.OrdinalIgnoreCase));

        public bool PaintZoneCell(string zoneId, Vector2Int position)
        {
            var zone = FindZone(zoneId);
            if (zone == null || !IsInBounds(position)) return false;
            zone.bounds = RectUnion(zone.bounds, new RectInt(position.x, position.y, 1, 1));
            MarkRuntimeQueryCacheDirty();
            return true;
        }

        public bool EraseZoneCell(string zoneId, Vector2Int position)
        {
            var zone = FindZone(zoneId);
            if (zone == null || !zone.bounds.Contains(position)) return false;
            if (zone.bounds.width == 1 && zone.bounds.height == 1) return RemoveZone(zoneId);
            if (position.x == zone.bounds.xMin) zone.bounds = new RectInt(zone.bounds.xMin + 1, zone.bounds.yMin, zone.bounds.width - 1, zone.bounds.height);
            else if (position.x == zone.bounds.xMax - 1) zone.bounds = new RectInt(zone.bounds.xMin, zone.bounds.yMin, zone.bounds.width - 1, zone.bounds.height);
            if (position.y == zone.bounds.yMin) zone.bounds = new RectInt(zone.bounds.xMin, zone.bounds.yMin + 1, zone.bounds.width, zone.bounds.height - 1);
            else if (position.y == zone.bounds.yMax - 1) zone.bounds = new RectInt(zone.bounds.xMin, zone.bounds.yMin, zone.bounds.width, zone.bounds.height - 1);
            MarkRuntimeQueryCacheDirty();
            return true;
        }

        private string GenerateUniqueLayerId(string displayName)
        {
            var stem = new string((displayName ?? "Layer").Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray()).Trim('_');
            if (string.IsNullOrWhiteSpace(stem)) stem = "Layer";
            var candidate = stem;
            var index = 1;
            while (FindLayer(candidate) != null) candidate = $"{stem}_{++index}";
            return candidate;
        }

        private string GenerateUniqueObjectId(string displayName) => GenerateUniqueId(displayName, "Object", FindObject);
        private string GenerateUniqueEntranceId(string displayName) => GenerateUniqueId(displayName, "Entrance", ResolveEntrance);
        private string GenerateUniqueExitId(string displayName) => GenerateUniqueId(displayName, "Exit", ResolveExit);

        private string GenerateUniqueId<T>(string displayName, string fallback, Func<string, T> finder) where T : class
        {
            var stem = new string((displayName ?? fallback).Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray()).Trim('_');
            if (string.IsNullOrWhiteSpace(stem)) stem = fallback;
            var candidate = stem;
            var index = 1;
            while (finder(candidate) != null) candidate = $"{stem}_{++index}";
            return candidate;
        }

        private string GenerateUniqueZoneId(string displayName)
        {
            var stem = new string((displayName ?? "Zone").Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray()).Trim('_');
            if (string.IsNullOrWhiteSpace(stem)) stem = "Zone";
            var candidate = stem;
            var index = 1;
            while (FindZone(candidate) != null) candidate = $"{stem}_{++index}";
            return candidate;
        }

        private static RPGMapObjectPlacement CloneObject(RPGMapObjectPlacement source) => new RPGMapObjectPlacement
        {
            objectId = source.objectId, displayName = source.displayName, prefab = source.prefab, addressableKey = source.addressableKey, gridPosition = source.gridPosition,
            worldOffset = source.worldOffset, rotationEuler = source.rotationEuler, scale = source.scale, snapToGrid = source.snapToGrid, spawnConditionKey = source.spawnConditionKey,
            persistentStateKey = source.persistentStateKey, category = source.category, facing = source.facing, blocksMovement = source.blocksMovement,
            metadata = source.metadata?.Select(entry => entry == null ? null : new RPGMapMetadataEntry { key = entry.key, value = entry.value }).ToList() ?? new List<RPGMapMetadataEntry>()
        };

        private static RPGMapTile CloneTile(RPGMapTile tile) => tile == null ? null : new RPGMapTile
        {
            position = tile.position,
            tileId = tile.tileId,
            rotationDegrees = tile.rotationDegrees,
            flipX = tile.flipX,
            flipY = tile.flipY,
            overrideCollision = tile.overrideCollision,
            blocksMovementOverride = tile.blocksMovementOverride,
            overrideMetadata = tile.overrideMetadata?.Select(entry => entry == null ? null : new RPGMapMetadataEntry { key = entry.key, value = entry.value }).ToList() ?? new List<RPGMapMetadataEntry>()
        };

        private IEnumerable<Vector2Int> EnumerateClamped(RectInt bounds)
        {
            var xMin = Mathf.Max(0, bounds.xMin);
            var yMin = Mathf.Max(0, bounds.yMin);
            var xMax = Mathf.Min(_size.x, bounds.xMax);
            var yMax = Mathf.Min(_size.y, bounds.yMax);
            for (var y = yMin; y < yMax; y++)
                for (var x = xMin; x < xMax; x++)
                    yield return new Vector2Int(x, y);
        }


        public IEnumerable<RPGMapZone> GetZonesAt(Vector2Int position, RPGMapZoneKind? kind = null, string tag = null)
        {
            EnsureRuntimeQueryCache();
            IEnumerable<RPGMapZone> source = _zonesByPosition.TryGetValue(position, out var cachedZones) ? cachedZones : Enumerable.Empty<RPGMapZone>();
            foreach (var zone in source.OrderByDescending(zone => zone?.priority ?? int.MinValue).ThenBy(zone => zone?.zoneId))
            {
                if (zone == null || (kind.HasValue && zone.kind != kind.Value) || !zone.bounds.Contains(position)) continue;
                if (!string.IsNullOrWhiteSpace(tag) && (zone.tags == null || !zone.tags.Any(value => string.Equals(value, tag, StringComparison.OrdinalIgnoreCase)))) continue;
                yield return zone;
            }
        }

        public RPGMapZone GetHighestPriorityZoneAt(Vector2Int position, RPGMapZoneKind? kind = null, string tag = null) => GetZonesAt(position, kind, tag).FirstOrDefault();

        public IEnumerable<RPGMapTile> GetTilesAt(Vector2Int position)
        {
            EnsureRuntimeQueryCache();
            return _tilesByPosition.TryGetValue(position, out var tiles) ? (IEnumerable<RPGMapTile>)tiles : Enumerable.Empty<RPGMapTile>();
        }

        public RPGMapTile GetLayerTile(string layerId, Vector2Int position)
        {
            EnsureRuntimeQueryCache();
            return !string.IsNullOrWhiteSpace(layerId) && _tilesByLayer.TryGetValue(layerId, out var layerTiles) && layerTiles.TryGetValue(position, out var tile) ? tile : null;
        }

        public IEnumerable<RPGMapObjectPlacement> GetObjectsInRegion(RectInt region, RPGMapObjectCategory? category = null)
        {
            EnsureRuntimeQueryCache();
            for (var y = region.yMin; y < region.yMax; y++)
            for (var x = region.xMin; x < region.xMax; x++)
                if (_objectsByPosition.TryGetValue(new Vector2Int(x, y), out var placements))
                    foreach (var placement in placements)
                        if (placement != null && (!category.HasValue || placement.category == category.Value)) yield return placement;
        }

        public RPGMapObjectPlacement FindObject(string objectId) => string.IsNullOrWhiteSpace(objectId) ? null : _objects.FirstOrDefault(placement => placement != null && string.Equals(placement.objectId, objectId, StringComparison.OrdinalIgnoreCase));
        public RPGMapEntrance ResolveEntrance(string entranceId) => string.IsNullOrWhiteSpace(entranceId) ? null : _entrances.FirstOrDefault(entrance => entrance != null && string.Equals(entrance.entranceId, entranceId, StringComparison.OrdinalIgnoreCase));
        public RPGMapExit ResolveExit(string exitId) => string.IsNullOrWhiteSpace(exitId) ? null : _exits.FirstOrDefault(exit => exit != null && string.Equals(exit.exitId, exitId, StringComparison.OrdinalIgnoreCase));
        public RPGMapConnection ResolveConnection(string connectionId) => string.IsNullOrWhiteSpace(connectionId) ? null : _connections.FirstOrDefault(connection => connection != null && string.Equals(connection.connectionId, connectionId, StringComparison.OrdinalIgnoreCase));

        public bool IsBlocked(Vector2Int position)
        {
            EnsureRuntimeQueryCache();
            return _blockedPositions.Contains(position);
        }

        public RPGMapCellMetadata GetCellMetadata(Vector2Int position)
        {
            var cell = new RPGMapCellMetadata { position = position, blocked = IsBlocked(position) };
            cell.tiles.AddRange(GetTilesAt(position));
            cell.zones.AddRange(GetZonesAt(position));
            cell.objects.AddRange(_objects.Where(placement => placement != null && placement.gridPosition == position));
            cell.entrances.AddRange(_entrances.Where(entrance => entrance != null && entrance.position == position));
            cell.exits.AddRange(_exits.Where(exit => exit != null && exit.position == position));
            foreach (var tile in cell.tiles)
                foreach (var entry in tile.overrideMetadata ?? Enumerable.Empty<RPGMapMetadataEntry>())
                    AddEffectiveMetadata(cell, RPGMapMetadataScope.Tile, tile.tileId, entry, 0);
            foreach (var zone in cell.zones)
                AddEffectiveMetadata(cell, RPGMapMetadataScope.Zone, zone.zoneId, new RPGMapMetadataEntry { key = "payloadId", value = zone.payloadId }, zone.priority);
            foreach (var placement in cell.objects)
                foreach (var entry in placement.metadata ?? Enumerable.Empty<RPGMapMetadataEntry>())
                    AddEffectiveMetadata(cell, RPGMapMetadataScope.Object, placement.objectId, entry, 0);
            foreach (var exit in cell.exits)
                AddEffectiveMetadata(cell, RPGMapMetadataScope.Connection, exit.exitId, new RPGMapMetadataEntry { key = "targetEntranceId", value = exit.targetEntranceId }, 0);
            return cell;
        }

        public RPGMapTile GetTopTile(Vector2Int position) => GetTilesAt(position).LastOrDefault();

        public RPGMapLayer FindLayer(string layerId) => string.IsNullOrWhiteSpace(layerId) ? null : _layers.FirstOrDefault(layer => layer != null && string.Equals(layer.layerId, layerId, StringComparison.OrdinalIgnoreCase));


        public void MarkRuntimeQueryCacheDirty() => _queryCacheDirty = true;

        public void BuildRuntimeQueryCache()
        {
            _tilesByLayer.Clear();
            _tilesByPosition.Clear();
            _zonesByPosition.Clear();
            _objectsByPosition.Clear();
            _blockedPositions.Clear();

            foreach (var layer in _layers.Where(layer => layer != null).OrderBy(layer => layer.renderOrder))
            {
                if (string.IsNullOrWhiteSpace(layer.layerId)) continue;
                var layerTiles = new Dictionary<Vector2Int, RPGMapTile>();
                foreach (var tile in layer.tiles ?? Enumerable.Empty<RPGMapTile>())
                {
                    if (tile == null) continue;
                    layerTiles[tile.position] = tile;
                    if (!_tilesByPosition.TryGetValue(tile.position, out var stackedTiles)) _tilesByPosition[tile.position] = stackedTiles = new List<RPGMapTile>();
                    stackedTiles.Add(tile);
                    var definition = _tileset?.FindTile(tile.tileId);
                    if ((tile.overrideCollision && tile.blocksMovementOverride) || (!tile.overrideCollision && (definition?.blocksMovement == true || layer.kind == RPGMapLayerKind.Collision))) _blockedPositions.Add(tile.position);
                }
                _tilesByLayer[layer.layerId] = layerTiles;
            }

            foreach (var zone in _zones.Where(zone => zone != null))
                foreach (var position in EnumerateClamped(zone.bounds))
                {
                    if (!_zonesByPosition.TryGetValue(position, out var zones)) _zonesByPosition[position] = zones = new List<RPGMapZone>();
                    zones.Add(zone);
                }

            foreach (var placement in _objects.Where(placement => placement != null))
            {
                if (!_objectsByPosition.TryGetValue(placement.gridPosition, out var placements)) _objectsByPosition[placement.gridPosition] = placements = new List<RPGMapObjectPlacement>();
                placements.Add(placement);
                if (placement.blocksMovement) _blockedPositions.Add(placement.gridPosition);
            }
            _queryCacheDirty = false;
        }

        private void EnsureRuntimeQueryCache()
        {
            if (_performanceSettings?.cacheRuntimeQueries == false) MarkRuntimeQueryCacheDirty();
            if (_queryCacheDirty) BuildRuntimeQueryCache();
        }

        public RPGValidationResult ValidateMap()
        {
            MigrateExpandedMapData();
            var result = new RPGValidationResult();
            if (_tileset == null) result.AddError("RPG_MAP_MISSING_TILESET", $"{name} has no tileset.", Id);
            if (_size.x <= 0 || _size.y <= 0) result.AddError("RPG_MAP_INVALID_SIZE", $"{name} must have a positive size.", Id);
            ValidateLayers(result);
            ValidateObjects(result);
            ValidateEntrancesAndExits(result);
            ValidateZones(result);
            return result;
        }

        private void ValidateLayers(RPGValidationResult result)
        {
            var layerIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var renderOrders = new HashSet<int>();
            var blockers = new HashSet<Vector2Int>();
            foreach (var layer in _layers)
            {
                if (layer == null) { result.AddWarning("RPG_MAP_NULL_LAYER", $"{name} contains an empty layer.", Id); continue; }
                if (string.IsNullOrWhiteSpace(layer.layerId)) result.AddError("RPG_MAP_EMPTY_LAYER_ID", $"{name} contains a layer without an id.", Id);
                else if (!layerIds.Add(layer.layerId)) result.AddError("RPG_MAP_DUPLICATE_LAYER_ID", $"{name} has duplicate layer id {layer.layerId}.", Id);
                if (!renderOrders.Add(layer.renderOrder)) result.AddWarning("RPG_MAP_DUPLICATE_LAYER_ORDER", $"{name} has duplicate render order {layer.renderOrder}.", Id);
                if (layer.opacity < 0f || layer.opacity > 1f) result.AddError("RPG_MAP_INVALID_LAYER_OPACITY", $"{name}/{layer.layerId} has opacity outside 0-1.", Id);
                foreach (var tile in layer.tiles ?? Enumerable.Empty<RPGMapTile>())
                {
                    if (tile == null) continue;
                    if (!IsInBounds(tile.position)) result.AddError("RPG_MAP_TILE_OUT_OF_BOUNDS", $"{name}/{layer.layerId} has a tile outside map bounds at {tile.position}.", Id);
                    if (_tileset != null && _tileset.FindTile(tile.tileId) == null) result.AddError("RPG_MAP_UNKNOWN_TILE", $"{name}/{layer.layerId} references missing tile {tile.tileId}.", Id);
                    if (layer.kind == RPGMapLayerKind.Collision && !blockers.Add(tile.position)) result.AddWarning("RPG_MAP_OVERLAPPING_BLOCKER", $"{name} has overlapping blockers at {tile.position}.", Id);
                    ValidateMetadata(tile.overrideMetadata, "RPG_MAP_INVALID_TILE_METADATA_KEY", $"{name}/{layer.layerId}/{tile.position}", result);
                }
            }
        }

        private void ValidateObjects(RPGValidationResult result)
        {
            var objectIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var placement in _objects)
            {
                if (placement == null) { result.AddWarning("RPG_MAP_NULL_OBJECT", $"{name} contains an empty object placement.", Id); continue; }
                if (string.IsNullOrWhiteSpace(placement.objectId)) result.AddError("RPG_MAP_EMPTY_OBJECT_ID", $"{name} contains an object without an id.", Id);
                else if (!objectIds.Add(placement.objectId)) result.AddError("RPG_MAP_DUPLICATE_OBJECT_ID", $"{name} has duplicate object id {placement.objectId}.", Id);
                if (!IsInBounds(placement.gridPosition)) result.AddError("RPG_MAP_OBJECT_OUT_OF_BOUNDS", $"{name}/{placement.objectId} is outside map bounds at {placement.gridPosition}.", Id);
                ValidateMetadata(placement.metadata, "RPG_MAP_INVALID_OBJECT_METADATA_KEY", $"{name}/{placement.objectId}", result);
            }
        }

        private void ValidateEntrancesAndExits(RPGValidationResult result)
        {
            var entranceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entrance in _entrances)
            {
                if (entrance == null) continue;
                if (string.IsNullOrWhiteSpace(entrance.entranceId)) result.AddError("RPG_MAP_EMPTY_ENTRANCE_ID", $"{name} contains an entrance without an id.", Id);
                else if (!entranceIds.Add(entrance.entranceId)) result.AddError("RPG_MAP_DUPLICATE_ENTRANCE_ID", $"{name} has duplicate entrance id {entrance.entranceId}.", Id);
                if (!IsInBounds(entrance.position)) result.AddError("RPG_MAP_ENTRANCE_OUT_OF_BOUNDS", $"{name}/{entrance.entranceId} is outside map bounds at {entrance.position}.", Id);
            }
            var exitIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var exit in _exits)
            {
                if (exit == null) continue;
                if (string.IsNullOrWhiteSpace(exit.exitId)) result.AddError("RPG_MAP_EMPTY_EXIT_ID", $"{name} contains an exit without an id.", Id);
                else if (!exitIds.Add(exit.exitId)) result.AddError("RPG_MAP_DUPLICATE_EXIT_ID", $"{name} has duplicate exit id {exit.exitId}.", Id);
                if (!IsInBounds(exit.position)) result.AddError("RPG_MAP_EXIT_OUT_OF_BOUNDS", $"{name}/{exit.exitId} is outside map bounds at {exit.position}.", Id);
                if (exit.targetMap == null && string.IsNullOrWhiteSpace(exit.targetMapId)) result.AddError("RPG_MAP_EXIT_MISSING_TARGET", $"{name}/{exit.exitId} has no target map.", Id);
                if (exit.targetMap != null && !string.IsNullOrWhiteSpace(exit.targetEntranceId) && exit.targetMap.ResolveEntrance(exit.targetEntranceId) == null) result.AddError("RPG_MAP_EXIT_MISSING_TARGET_ENTRANCE", $"{name}/{exit.exitId} targets missing entrance {exit.targetEntranceId}.", Id);
            }
            foreach (var connection in _connections)
                if (connection != null && connection.targetMap == null && string.IsNullOrWhiteSpace(connection.targetMapId)) result.AddError("RPG_MAP_BROKEN_CONNECTION", $"{name}/{connection.connectionId} has no target map.", Id);
        }

        private void ValidateZones(RPGValidationResult result)
        {
            var zoneIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var zone in _zones)
            {
                if (zone == null) continue;
                if (string.IsNullOrWhiteSpace(zone.zoneId)) result.AddError("RPG_MAP_EMPTY_ZONE_ID", $"{name} contains a zone without an id.", Id);
                else if (!zoneIds.Add(zone.zoneId)) result.AddError("RPG_MAP_DUPLICATE_ZONE_ID", $"{name} has duplicate zone id {zone.zoneId}.", Id);
                if (!IsRectInBounds(zone.bounds)) result.AddError("RPG_MAP_ZONE_OUT_OF_BOUNDS", $"{name}/{zone.zoneId} is outside map bounds.", Id);
            }
        }

        private void ValidateMetadata(IEnumerable<RPGMapMetadataEntry> metadata, string code, string owner, RPGValidationResult result)
        {
            foreach (var entry in metadata ?? Enumerable.Empty<RPGMapMetadataEntry>())
                if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.key.Any(char.IsWhiteSpace)) result.AddError(code, $"{owner} contains invalid metadata key.", Id);
        }

        private bool IsInBounds(Vector2Int position) => position.x >= 0 && position.y >= 0 && position.x < _size.x && position.y < _size.y;
        private bool IsRectInBounds(RectInt rect) => rect.width > 0 && rect.height > 0 && IsInBounds(rect.min) && rect.xMax <= _size.x && rect.yMax <= _size.y;
        private RectInt ClampRect(RectInt rect)
        {
            var xMin = Mathf.Clamp(rect.xMin, 0, Mathf.Max(0, _size.x - 1));
            var yMin = Mathf.Clamp(rect.yMin, 0, Mathf.Max(0, _size.y - 1));
            var xMax = Mathf.Clamp(rect.xMax, xMin + 1, _size.x);
            var yMax = Mathf.Clamp(rect.yMax, yMin + 1, _size.y);
            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }
        private static RectInt RectUnion(RectInt a, RectInt b)
        {
            var xMin = Mathf.Min(a.xMin, b.xMin);
            var yMin = Mathf.Min(a.yMin, b.yMin);
            var xMax = Mathf.Max(a.xMax, b.xMax);
            var yMax = Mathf.Max(a.yMax, b.yMax);
            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }
        private static void AddEffectiveMetadata(RPGMapCellMetadata cell, RPGMapMetadataScope scope, string ownerId, RPGMapMetadataEntry entry, int priority)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.key)) return;
            cell.metadata.Add(new RPGMapEffectiveMetadataEntry { scope = scope, ownerId = ownerId, key = entry.key, value = entry.value, priority = priority });
        }
    }
}
