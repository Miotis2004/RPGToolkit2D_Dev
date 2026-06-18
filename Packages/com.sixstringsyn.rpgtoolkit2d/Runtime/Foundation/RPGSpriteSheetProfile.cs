using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Foundation
{
    [CreateAssetMenu(fileName = "RPGSpriteSheetProfile", menuName = "RPG Toolkit/Foundation/Sprite Sheet Profile")]
    public sealed class RPGSpriteSheetProfile : ScriptableObject
    {
        [SerializeField] private RPGContentKind _contentKind = RPGContentKind.SpriteSheet;
        [SerializeField] private Vector2Int _cellSize = new Vector2Int(16, 16);
        [SerializeField] private Vector2Int _padding;
        [SerializeField] private Vector2 _pivot = new Vector2(0.5f, 0f);
        [SerializeField] private int _pixelsPerUnit = 16;
        [SerializeField] private bool _requiresConsistentCellSize = true;

        public RPGContentKind ContentKind => _contentKind;
        public Vector2Int CellSize => _cellSize;
        public Vector2Int Padding => _padding;
        public Vector2 Pivot => _pivot;
        public int PixelsPerUnit => Math.Max(1, _pixelsPerUnit);
        public bool RequiresConsistentCellSize => _requiresConsistentCellSize;
    }
}
