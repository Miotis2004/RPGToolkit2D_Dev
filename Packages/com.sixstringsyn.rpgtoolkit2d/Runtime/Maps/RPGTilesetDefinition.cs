using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Maps
{
    [Serializable]
    public sealed class RPGTileDefinition
    {
        public string tileId;
        public Sprite sprite;
        public bool blocksMovement;
        public bool overhead;
        public string terrainTag;
    }

    [CreateAssetMenu(fileName = "RPGTilesetDefinition", menuName = "RPG Toolkit/Maps/Tileset")]
    public sealed class RPGTilesetDefinition : RPGObject
    {
        [SerializeField] private RPGSpriteSheetAsset _spriteSheet;
        [SerializeField] private List<RPGTileDefinition> _tiles = new List<RPGTileDefinition>();

        public RPGSpriteSheetAsset SpriteSheet => _spriteSheet;
        public IReadOnlyList<RPGTileDefinition> Tiles => _tiles;

        public RPGTileDefinition FindTile(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId)) return null;
            return _tiles.Find(tile => tile != null && string.Equals(tile.tileId, tileId, StringComparison.OrdinalIgnoreCase));
        }

        public RPGValidationResult ValidateTileset()
        {
            var result = new RPGValidationResult();
            if (_spriteSheet == null) result.AddWarning("RPG_TILESET_MISSING_SPRITESHEET", $"{name} is not linked to a sprite sheet.", Id);
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tile in _tiles)
            {
                if (tile == null) { result.AddWarning("RPG_TILESET_NULL_TILE", $"{name} contains an empty tile entry.", Id); continue; }
                if (string.IsNullOrWhiteSpace(tile.tileId)) result.AddError("RPG_TILESET_EMPTY_TILE_ID", $"{name} contains a tile without an id.", Id);
                else if (!ids.Add(tile.tileId)) result.AddError("RPG_TILESET_DUPLICATE_TILE_ID", $"{name} has duplicate tile id {tile.tileId}.", Id);
                if (tile.sprite == null) result.AddWarning("RPG_TILESET_MISSING_SPRITE", $"{name}/{tile.tileId} has no sprite assigned.", Id);
            }
            return result;
        }
    }
}
