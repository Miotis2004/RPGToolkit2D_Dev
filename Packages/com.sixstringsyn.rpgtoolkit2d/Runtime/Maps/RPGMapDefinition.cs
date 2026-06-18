using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Maps
{
    public enum RPGMapLayerKind { Ground, Decoration, Collision, Overhead, Trigger }
    public enum RPGMapZoneKind { Region, Encounter, Weather, Lighting, Spawn }
    public enum RPGMapObjectCategory { NPC, Monster, Item, Chest, Door, SavePoint, Decoration, Custom }
    public enum RPGMapDirection { None, North, East, South, West }
    public enum RPGMapTransitionKind { Instant, Fade, Door, Stairs, Custom }

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
        public GameObject prefab;
        public string addressableKey;
        public Vector2Int gridPosition;
        public Vector3 worldOffset;
        public Vector3 rotationEuler;
        public Vector3 scale = Vector3.one;
        public string spawnConditionKey;
        public string persistentStateKey;
        public RPGMapObjectCategory category = RPGMapObjectCategory.Custom;
        public bool blocksMovement;
        public List<RPGMapMetadataEntry> metadata = new List<RPGMapMetadataEntry>();
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

        public Vector2Int Size => _size;
        public RPGTilesetDefinition Tileset => _tileset;
        public IReadOnlyList<RPGMapLayer> Layers => _layers;
        public IReadOnlyList<RPGMapZone> Zones => _zones;
        public IReadOnlyList<RPGMapConnection> Connections => _connections;
        public IReadOnlyList<RPGMapObjectPlacement> Objects => _objects;
        public IReadOnlyList<RPGMapEntrance> Entrances => _entrances;
        public IReadOnlyList<RPGMapExit> Exits => _exits;

        public void OnBeforeSerialize() => MigrateExpandedMapData();
        public void OnAfterDeserialize() => MigrateExpandedMapData();

        public void MigrateExpandedMapData()
        {
            if (_layers == null) _layers = new List<RPGMapLayer>();
            if (_zones == null) _zones = new List<RPGMapZone>();
            if (_connections == null) _connections = new List<RPGMapConnection>();
            if (_objects == null) _objects = new List<RPGMapObjectPlacement>();
            if (_entrances == null) _entrances = new List<RPGMapEntrance>();
            if (_exits == null) _exits = new List<RPGMapExit>();
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
        }

        public IEnumerable<RPGMapZone> GetZonesAt(Vector2Int position, RPGMapZoneKind? kind = null, string tag = null)
        {
            foreach (var zone in _zones)
            {
                if (zone == null || (kind.HasValue && zone.kind != kind.Value) || !zone.bounds.Contains(position)) continue;
                if (!string.IsNullOrWhiteSpace(tag) && (zone.tags == null || !zone.tags.Any(value => string.Equals(value, tag, StringComparison.OrdinalIgnoreCase)))) continue;
                yield return zone;
            }
        }

        public IEnumerable<RPGMapTile> GetTilesAt(Vector2Int position)
        {
            foreach (var layer in _layers.OrderBy(layer => layer?.renderOrder ?? 0))
                if (layer?.tiles != null)
                    foreach (var tile in layer.tiles)
                        if (tile != null && tile.position == position) yield return tile;
        }

        public RPGMapTile GetLayerTile(string layerId, Vector2Int position)
        {
            var layer = FindLayer(layerId);
            return layer?.tiles?.LastOrDefault(tile => tile != null && tile.position == position);
        }

        public IEnumerable<RPGMapObjectPlacement> GetObjectsInRegion(RectInt region, RPGMapObjectCategory? category = null)
        {
            foreach (var placement in _objects)
                if (placement != null && (!category.HasValue || placement.category == category.Value) && region.Contains(placement.gridPosition)) yield return placement;
        }

        public RPGMapEntrance ResolveEntrance(string entranceId) => string.IsNullOrWhiteSpace(entranceId) ? null : _entrances.FirstOrDefault(entrance => entrance != null && string.Equals(entrance.entranceId, entranceId, StringComparison.OrdinalIgnoreCase));
        public RPGMapExit ResolveExit(string exitId) => string.IsNullOrWhiteSpace(exitId) ? null : _exits.FirstOrDefault(exit => exit != null && string.Equals(exit.exitId, exitId, StringComparison.OrdinalIgnoreCase));
        public RPGMapConnection ResolveConnection(string connectionId) => string.IsNullOrWhiteSpace(connectionId) ? null : _connections.FirstOrDefault(connection => connection != null && string.Equals(connection.connectionId, connectionId, StringComparison.OrdinalIgnoreCase));

        public bool IsBlocked(Vector2Int position)
        {
            foreach (var tile in GetTilesAt(position))
            {
                if (tile.overrideCollision) { if (tile.blocksMovementOverride) return true; continue; }
                var definition = _tileset?.FindTile(tile.tileId);
                if (definition?.blocksMovement == true) return true;
            }
            foreach (var layer in _layers)
            {
                if (layer == null || layer.kind != RPGMapLayerKind.Collision) continue;
                if (layer.tiles != null && layer.tiles.Any(tile => tile != null && tile.position == position && (!tile.overrideCollision || tile.blocksMovementOverride))) return true;
            }
            return _objects.Any(placement => placement != null && placement.blocksMovement && placement.gridPosition == position);
        }

        public RPGMapTile GetTopTile(Vector2Int position) => _layers.OrderBy(layer => layer?.renderOrder ?? 0).SelectMany(layer => layer?.tiles ?? Enumerable.Empty<RPGMapTile>()).LastOrDefault(tile => tile != null && tile.position == position);

        public RPGMapLayer FindLayer(string layerId) => string.IsNullOrWhiteSpace(layerId) ? null : _layers.FirstOrDefault(layer => layer != null && string.Equals(layer.layerId, layerId, StringComparison.OrdinalIgnoreCase));

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
            foreach (var zone in _zones)
            {
                if (zone == null) continue;
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
    }
}
