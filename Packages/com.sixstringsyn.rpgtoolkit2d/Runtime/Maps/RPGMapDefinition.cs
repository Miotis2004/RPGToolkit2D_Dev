using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Maps
{
    public enum RPGMapLayerKind { Ground, Decoration, Collision, Overhead, Trigger }
    public enum RPGMapZoneKind { Region, Encounter, Weather, Lighting, Spawn }

    [Serializable]
    public sealed class RPGMapTile
    {
        public Vector2Int position;
        public string tileId;
    }

    [Serializable]
    public sealed class RPGMapLayer
    {
        public string layerId = "Layer";
        public RPGMapLayerKind kind;
        public List<RPGMapTile> tiles = new List<RPGMapTile>();
    }

    [Serializable]
    public sealed class RPGMapZone
    {
        public string zoneId;
        public RPGMapZoneKind kind;
        public RectInt bounds;
        public string payloadId;
    }

    [Serializable]
    public sealed class RPGMapConnection
    {
        public string connectionId;
        public RPGMapDefinition targetMap;
        public string targetEntranceId;
    }

    [CreateAssetMenu(fileName = "RPGMapDefinition", menuName = "RPG Toolkit/Maps/Map")]
    public sealed class RPGMapDefinition : RPGObject
    {
        [SerializeField] private Vector2Int _size = new Vector2Int(32, 32);
        [SerializeField] private RPGTilesetDefinition _tileset;
        [SerializeField] private List<RPGMapLayer> _layers = new List<RPGMapLayer>();
        [SerializeField] private List<RPGMapZone> _zones = new List<RPGMapZone>();
        [SerializeField] private List<RPGMapConnection> _connections = new List<RPGMapConnection>();

        public Vector2Int Size => _size;
        public RPGTilesetDefinition Tileset => _tileset;
        public IReadOnlyList<RPGMapLayer> Layers => _layers;
        public IReadOnlyList<RPGMapZone> Zones => _zones;
        public IReadOnlyList<RPGMapConnection> Connections => _connections;

        public IEnumerable<RPGMapZone> GetZonesAt(Vector2Int position, RPGMapZoneKind? kind = null)
        {
            foreach (var zone in _zones)
            {
                if (zone != null && (!kind.HasValue || zone.kind == kind.Value) && zone.bounds.Contains(position)) yield return zone;
            }
        }

        public bool IsBlocked(Vector2Int position)
        {
            foreach (var layer in _layers)
            {
                if (layer == null || layer.kind != RPGMapLayerKind.Collision) continue;
                foreach (var tile in layer.tiles)
                    if (tile != null && tile.position == position) return true;
            }
            var ground = GetTopTile(position);
            return ground != null && _tileset != null && _tileset.FindTile(ground.tileId)?.blocksMovement == true;
        }

        public RPGMapTile GetTopTile(Vector2Int position)
        {
            for (var i = _layers.Count - 1; i >= 0; i--)
            {
                var layer = _layers[i];
                if (layer == null) continue;
                for (var j = layer.tiles.Count - 1; j >= 0; j--)
                    if (layer.tiles[j] != null && layer.tiles[j].position == position) return layer.tiles[j];
            }
            return null;
        }

        public RPGValidationResult ValidateMap()
        {
            var result = new RPGValidationResult();
            if (_tileset == null) result.AddError("RPG_MAP_MISSING_TILESET", $"{name} has no tileset.", Id);
            if (_size.x <= 0 || _size.y <= 0) result.AddError("RPG_MAP_INVALID_SIZE", $"{name} must have a positive size.", Id);
            var blockers = new HashSet<Vector2Int>();
            foreach (var layer in _layers)
            {
                if (layer == null) { result.AddWarning("RPG_MAP_NULL_LAYER", $"{name} contains an empty layer.", Id); continue; }
                foreach (var tile in layer.tiles)
                {
                    if (tile == null) continue;
                    if (tile.position.x < 0 || tile.position.y < 0 || tile.position.x >= _size.x || tile.position.y >= _size.y) result.AddError("RPG_MAP_TILE_OUT_OF_BOUNDS", $"{name}/{layer.layerId} has a tile outside map bounds at {tile.position}.", Id);
                    if (_tileset != null && _tileset.FindTile(tile.tileId) == null) result.AddError("RPG_MAP_UNKNOWN_TILE", $"{name}/{layer.layerId} references missing tile {tile.tileId}.", Id);
                    if (layer.kind == RPGMapLayerKind.Collision && !blockers.Add(tile.position)) result.AddWarning("RPG_MAP_OVERLAPPING_BLOCKER", $"{name} has overlapping blockers at {tile.position}.", Id);
                }
            }
            foreach (var connection in _connections)
                if (connection != null && connection.targetMap == null) result.AddError("RPG_MAP_BROKEN_CONNECTION", $"{name}/{connection.connectionId} has no target map.", Id);
            return result;
        }
    }
}
