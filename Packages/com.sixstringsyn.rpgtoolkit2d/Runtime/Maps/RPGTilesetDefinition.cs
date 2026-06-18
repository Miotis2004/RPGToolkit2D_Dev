using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Maps
{
    public enum RPGTileCategory { Terrain, Water, Wall, Decoration, Object, Hazard, Entrance, Custom }
    public enum RPGTileCollisionShapeMode { None, Full, SpritePhysicsShape, CustomPolygon }
    public enum RPGTileLayerKind { Ground, Detail, Collision, Overhead, Trigger, Custom }

    [Serializable]
    public sealed class RPGTileAnimationMetadata
    {
        public bool enabled;
        public List<string> frameIds = new List<string>();
        public float framesPerSecond = 8f;
        public bool loop = true;
    }

    [Serializable]
    public sealed class RPGTileAutotileMetadata
    {
        public bool enabled;
        public string autotileSetId;
        public List<string> neighborMaskFrameIds = new List<string>();
        public List<string> wangEdgeTags = new List<string>();
        public List<string> wangCornerTags = new List<string>();
    }

    [Serializable]
    public sealed class RPGTileCustomMetadata
    {
        public string key;
        public string value;
    }

    [Serializable]
    public sealed class RPGTilePaletteDefinition
    {
        public string paletteId;
        public string displayName;
        public List<string> categoryFilterTags = new List<string>();
        public List<string> orderedTileIds = new List<string>();
    }

    [Serializable]
    public sealed class RPGTileDefinition
    {
        public string tileId;
        public Sprite sprite;
        public bool blocksMovement;
        public bool overhead;
        public string terrainTag;
        public string sourceFrameId;
        public RPGTileCategory category = RPGTileCategory.Terrain;
        public float walkCost = 1f;
        public RPGTileCollisionShapeMode collisionShapeMode = RPGTileCollisionShapeMode.None;
        public List<string> terrainTags = new List<string>();
        public RPGTileAutotileMetadata autotile = new RPGTileAutotileMetadata();
        public RPGTileAnimationMetadata animation = new RPGTileAnimationMetadata();
        public RPGTileLayerKind defaultLayerKind = RPGTileLayerKind.Ground;
        public List<RPGTileCustomMetadata> customMetadata = new List<RPGTileCustomMetadata>();

        public IEnumerable<string> AllTerrainTags()
        {
            if (!string.IsNullOrWhiteSpace(terrainTag)) yield return terrainTag;
            if (terrainTags == null) yield break;
            foreach (var tag in terrainTags)
                if (!string.IsNullOrWhiteSpace(tag)) yield return tag;
        }
    }

    [CreateAssetMenu(fileName = "RPGTilesetDefinition", menuName = "RPG Toolkit/Maps/Tileset")]
    public sealed class RPGTilesetDefinition : RPGObject
    {
        [SerializeField] private RPGSpriteSheetAsset _spriteSheet;
        [SerializeField] private List<RPGTileDefinition> _tiles = new List<RPGTileDefinition>();
        [SerializeField] private List<RPGTilePaletteDefinition> _palettes = new List<RPGTilePaletteDefinition>();

        public RPGSpriteSheetAsset SpriteSheet => _spriteSheet;
        public IReadOnlyList<RPGTileDefinition> Tiles => _tiles;
        public IReadOnlyList<RPGTilePaletteDefinition> Palettes => _palettes;

        public void Configure(RPGSpriteSheetAsset spriteSheet) => _spriteSheet = spriteSheet;
        public void SetTiles(IEnumerable<RPGTileDefinition> tiles) => _tiles = tiles?.Where(tile => tile != null).ToList() ?? new List<RPGTileDefinition>();
        public void SetPalettes(IEnumerable<RPGTilePaletteDefinition> palettes) => _palettes = palettes?.Where(palette => palette != null).ToList() ?? new List<RPGTilePaletteDefinition>();

        public RPGTileDefinition FindTile(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId)) return null;
            return _tiles.Find(tile => tile != null && string.Equals(tile.tileId, tileId, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<RPGTileDefinition> FindTilesByCategory(RPGTileCategory category) => _tiles.Where(tile => tile != null && tile.category == category);

        public IEnumerable<RPGTileDefinition> FindTilesByTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return Enumerable.Empty<RPGTileDefinition>();
            return _tiles.Where(tile => tile != null && tile.AllTerrainTags().Any(value => string.Equals(value, tag, StringComparison.OrdinalIgnoreCase)));
        }

        public RPGSpriteFrameMetadata GetSourceFrame(RPGTileDefinition tile) => tile == null || _spriteSheet == null ? null : _spriteSheet.FindFrame(tile.sourceFrameId);
        public Sprite GetSourceSprite(RPGTileDefinition tile) => tile?.sprite != null ? tile.sprite : GetSourceFrame(tile)?.sprite;

        public RPGValidationResult ValidateTileset()
        {
            var result = new RPGValidationResult();
            if (_spriteSheet == null) result.AddWarning("RPG_TILESET_MISSING_SPRITESHEET", $"{name} is not linked to a sprite sheet.", Id);
            var frameIds = _spriteSheet == null ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : new HashSet<string>(_spriteSheet.Frames.Where(frame => frame != null && !string.IsNullOrWhiteSpace(frame.frameId)).Select(frame => frame.frameId), StringComparer.OrdinalIgnoreCase);
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var sprites = new HashSet<Sprite>();
            foreach (var tile in _tiles)
            {
                if (tile == null) { result.AddWarning("RPG_TILESET_NULL_TILE", $"{name} contains an empty tile entry.", Id); continue; }
                if (string.IsNullOrWhiteSpace(tile.tileId)) result.AddError("RPG_TILESET_EMPTY_TILE_ID", $"{name} contains a tile without an id.", Id);
                else if (!ids.Add(tile.tileId)) result.AddError("RPG_TILESET_DUPLICATE_TILE_ID", $"{name} has duplicate tile id {tile.tileId}.", Id);
                if (tile.sprite == null) result.AddWarning("RPG_TILESET_MISSING_SPRITE", $"{name}/{tile.tileId} has no sprite assigned.", Id);
                else if (!sprites.Add(tile.sprite)) result.AddWarning("RPG_TILESET_DUPLICATE_TILE_SPRITE", $"{name}/{tile.tileId} reuses a sprite reference.", Id);
                if (!string.IsNullOrWhiteSpace(tile.sourceFrameId) && _spriteSheet != null && !frameIds.Contains(tile.sourceFrameId)) result.AddError("RPG_TILESET_UNKNOWN_FRAME_ID", $"{name}/{tile.tileId} references missing frame {tile.sourceFrameId}.", Id);
                if (tile.blocksMovement && tile.defaultLayerKind == RPGTileLayerKind.Overhead) result.AddWarning("RPG_TILESET_COLLISION_LAYER_CONFLICT", $"{name}/{tile.tileId} is blocking but defaults to an overhead layer.", Id);
                if (tile.animation?.enabled == true && (tile.animation.frameIds == null || tile.animation.frameIds.Count == 0)) result.AddError("RPG_TILESET_ANIMATION_MISSING_FRAMES", $"{name}/{tile.tileId} is animated but has no animation frames.", Id);
                if (tile.animation?.enabled == true && _spriteSheet != null && tile.animation.frameIds.Any(frameId => string.IsNullOrWhiteSpace(frameId) || !frameIds.Contains(frameId))) result.AddError("RPG_TILESET_ANIMATION_UNKNOWN_FRAME", $"{name}/{tile.tileId} has animation frames missing from the sprite sheet.", Id);
                if (tile.autotile?.enabled == true && (string.IsNullOrWhiteSpace(tile.autotile.autotileSetId) || tile.autotile.neighborMaskFrameIds == null || tile.autotile.neighborMaskFrameIds.Count == 0)) result.AddError("RPG_TILESET_AUTOTILE_INCOMPLETE", $"{name}/{tile.tileId} has incomplete autotile metadata.", Id);
            }
            var paletteIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var palette in _palettes)
            {
                if (palette == null) { result.AddWarning("RPG_TILESET_NULL_PALETTE", $"{name} contains an empty palette entry.", Id); continue; }
                if (string.IsNullOrWhiteSpace(palette.paletteId)) result.AddError("RPG_TILESET_EMPTY_PALETTE_ID", $"{name} contains a palette without an id.", Id);
                else if (!paletteIds.Add(palette.paletteId)) result.AddError("RPG_TILESET_DUPLICATE_PALETTE_ID", $"{name} has duplicate palette id {palette.paletteId}.", Id);
                foreach (var tileId in palette.orderedTileIds ?? Enumerable.Empty<string>())
                    if (!ids.Contains(tileId)) result.AddError("RPG_TILESET_INVALID_PALETTE_TILE", $"{name}/{palette.paletteId} references missing tile {tileId}.", Id);
            }
            return result;
        }
    }
}
