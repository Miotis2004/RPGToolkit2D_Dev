using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Maps
{
    [Serializable]
    public sealed class RPGMapStampPlacement
    {
        public Vector2Int offset;
        public string tileId;
        public int rotationDegrees;
        public bool flipX;
        public bool flipY;
    }

    [CreateAssetMenu(fileName = "RPGMapStampDefinition", menuName = "RPG Toolkit/Maps/Stamp")]
    public sealed class RPGMapStampDefinition : RPGObject
    {
        [SerializeField] private Vector2Int _size = Vector2Int.one;
        [SerializeField] private List<RPGMapStampPlacement> _tiles = new List<RPGMapStampPlacement>();
        [SerializeField] private List<string> _paletteTags = new List<string>();

        public Vector2Int Size => _size;
        public IReadOnlyList<RPGMapStampPlacement> Tiles => _tiles;
        public IReadOnlyList<string> PaletteTags => _paletteTags;

        public void Configure(Vector2Int size, IEnumerable<RPGMapStampPlacement> tiles, IEnumerable<string> paletteTags = null)
        {
            _size = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
            _tiles = tiles?.Where(tile => tile != null && !string.IsNullOrWhiteSpace(tile.tileId)).ToList() ?? new List<RPGMapStampPlacement>();
            _paletteTags = paletteTags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
        }

        public IEnumerable<RPGMapTile> BuildTiles(Vector2Int anchor)
        {
            foreach (var tile in _tiles ?? Enumerable.Empty<RPGMapStampPlacement>())
            {
                if (tile == null || string.IsNullOrWhiteSpace(tile.tileId)) continue;
                yield return new RPGMapTile
                {
                    position = anchor + tile.offset,
                    tileId = tile.tileId,
                    rotationDegrees = tile.rotationDegrees,
                    flipX = tile.flipX,
                    flipY = tile.flipY
                };
            }
        }
    }
}
