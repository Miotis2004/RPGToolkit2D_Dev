using System;
using System.Collections.Generic;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Foundation
{
    public enum RPGSpriteSheetSlicingMode { Grid, CustomRectangles, UnitySpriteMetadata, ExternalJson }
    public enum RPGSpriteSheetOrientation { TopLeft, TopRight, BottomLeft, BottomRight }
    public enum RPGSpriteSheetOrdering { RowMajor, ColumnMajor }

    [Serializable]
    public sealed class RPGSpriteAnimationGroupRule
    {
        public string groupId;
        public string frameIdPrefix;
        public int framesPerGroup = 1;
    }

    [CreateAssetMenu(fileName = "RPGSpriteSheetProfile", menuName = "RPG Toolkit/Foundation/Sprite Sheet Profile")]
    public sealed class RPGSpriteSheetProfile : ScriptableObject
    {
        [SerializeField] private RPGContentKind _contentKind = RPGContentKind.SpriteSheet;
        [SerializeField] private RPGSpriteSheetSlicingMode _slicingMode = RPGSpriteSheetSlicingMode.Grid;
        [SerializeField] private Vector2Int _cellSize = new Vector2Int(16, 16);
        [SerializeField] private Vector2Int _padding;
        [SerializeField] private Vector2Int _margin;
        [SerializeField] private Vector2Int _spacing;
        [SerializeField] private Vector2 _pivot = new Vector2(0.5f, 0f);
        [SerializeField] private int _pixelsPerUnit = 16;
        [SerializeField] private bool _requiresConsistentCellSize = true;
        [SerializeField] private string _defaultFrameNamingPattern = "{sheet}_{y}_{x}";
        [SerializeField] private RPGSpriteSheetOrientation _orientation = RPGSpriteSheetOrientation.TopLeft;
        [SerializeField] private RPGSpriteSheetOrdering _ordering = RPGSpriteSheetOrdering.RowMajor;
        [SerializeField] private bool _defaultBlocksMovement;
        [SerializeField] private string[] _defaultTags = Array.Empty<string>();
        [SerializeField] private RPGSpriteAnimationGroupRule[] _animationGroupRules = Array.Empty<RPGSpriteAnimationGroupRule>();

        public RPGContentKind ContentKind => _contentKind;
        public RPGSpriteSheetSlicingMode SlicingMode => _slicingMode;
        public Vector2Int CellSize => _cellSize;
        public Vector2Int Padding => _padding;
        public Vector2Int Margin => _margin;
        public Vector2Int Spacing => _spacing;
        public Vector2 Pivot => _pivot;
        public int PixelsPerUnit => Math.Max(1, _pixelsPerUnit);
        public bool RequiresConsistentCellSize => _requiresConsistentCellSize;
        public string DefaultFrameNamingPattern => string.IsNullOrWhiteSpace(_defaultFrameNamingPattern) ? "{sheet}_{index}" : _defaultFrameNamingPattern;
        public RPGSpriteSheetOrientation Orientation => _orientation;
        public RPGSpriteSheetOrdering Ordering => _ordering;
        public bool DefaultBlocksMovement => _defaultBlocksMovement;
        public IReadOnlyList<string> DefaultTags => _defaultTags;
        public IReadOnlyList<RPGSpriteAnimationGroupRule> AnimationGroupRules => _animationGroupRules;
    }
}
