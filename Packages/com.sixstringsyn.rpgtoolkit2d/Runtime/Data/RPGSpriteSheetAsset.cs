using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Data
{
    [Flags]
    public enum RPGSpriteTileFlags { None = 0, BlocksMovement = 1, BlocksSight = 2, Water = 4, Overhead = 8, Animated = 16 }

    [Serializable]
    public sealed class RPGSpriteFrameMetadata
    {
        public string frameId;
        public Sprite sprite;
        public Vector2Int gridPosition;
        public RectInt sourceRect;
        public Rect normalizedUv;
        public Vector2 pivot = new Vector2(0.5f, 0f);
        public bool blocksMovement;
        public RPGSpriteTileFlags defaultTileFlags;
        public List<string> tags = new List<string>();
        public string frameGroupId;
        public string animationKey;
        public int animationFrame;
        public string addressKey;
        public RPGId linkedItemId;
    }

    [CreateAssetMenu(fileName = "RPGSpriteSheetAsset", menuName = "RPG Toolkit/Data/Sprite Sheet Asset")]
    public sealed class RPGSpriteSheetAsset : RPGObject
    {
        [SerializeField] private Texture2D _sourceTexture;
        [SerializeField] private RPGSpriteSheetProfile _profile;
        [SerializeField] private List<RPGSpriteFrameMetadata> _frames = new List<RPGSpriteFrameMetadata>();
        public Texture2D SourceTexture => _sourceTexture;
        public RPGSpriteSheetProfile Profile => _profile;
        public IReadOnlyList<RPGSpriteFrameMetadata> Frames => _frames;

        public void Configure(Texture2D sourceTexture, RPGSpriteSheetProfile profile)
        {
            _sourceTexture = sourceTexture;
            _profile = profile;
        }

        public void SetFrames(IEnumerable<RPGSpriteFrameMetadata> frames)
        {
            _frames = frames?.Where(frame => frame != null).ToList() ?? new List<RPGSpriteFrameMetadata>();
        }

        public RPGSpriteFrameMetadata FindFrame(string frameId) => _frames.FirstOrDefault(frame => frame != null && string.Equals(frame.frameId, frameId, StringComparison.OrdinalIgnoreCase));
        public IEnumerable<RPGSpriteFrameMetadata> GetFramesByTag(string tag) => string.IsNullOrWhiteSpace(tag) ? Enumerable.Empty<RPGSpriteFrameMetadata>() : _frames.Where(frame => frame?.tags != null && frame.tags.Any(value => string.Equals(value, tag, StringComparison.OrdinalIgnoreCase)));
        public IEnumerable<RPGSpriteFrameMetadata> GetFramesByGroup(string groupId) => string.IsNullOrWhiteSpace(groupId) ? Enumerable.Empty<RPGSpriteFrameMetadata>() : _frames.Where(frame => frame != null && string.Equals(frame.frameGroupId, groupId, StringComparison.OrdinalIgnoreCase));
        public IEnumerable<RPGSpriteFrameMetadata> GetFramesByContentKind(RPGContentKind kind) => _profile != null && _profile.ContentKind == kind ? _frames.Where(frame => frame != null) : Enumerable.Empty<RPGSpriteFrameMetadata>();

        public RPGValidationResult ValidateSpriteSheet()
        {
            var result = new RPGValidationResult();
            if (_sourceTexture == null) result.AddError("RPG_SPRITESHEET_MISSING_TEXTURE", $"{name} has no source texture.", Id);
            if (_profile == null) result.AddError("RPG_SPRITESHEET_MISSING_PROFILE", $"{name} has no slicing profile.", Id);
            else
            {
                if (_profile.CellSize.x <= 0 || _profile.CellSize.y <= 0) result.AddError("RPG_SPRITESHEET_INVALID_CELL_SIZE", $"{name} uses an invalid profile cell size.", Id);
            }

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var gridPositions = new HashSet<Vector2Int>();
            var sprites = new HashSet<Sprite>();
            foreach (var frame in _frames)
            {
                if (frame == null) { result.AddWarning("RPG_SPRITESHEET_NULL_FRAME", $"{name} contains an empty frame.", Id); continue; }
                if (frame.sprite == null && _profile != null && _profile.SlicingMode == RPGSpriteSheetSlicingMode.UnitySpriteMetadata) result.AddWarning("RPG_SPRITESHEET_MISSING_SPRITE", $"{name}/{frame.frameId} has no sprite assigned.", Id);
                if (frame.sprite != null && !sprites.Add(frame.sprite)) result.AddWarning("RPG_SPRITESHEET_DUPLICATE_SPRITE", $"{name}/{frame.frameId} reuses a sprite reference.", Id);
                if (frame.sprite != null && _profile != null && Math.Abs(frame.sprite.pixelsPerUnit - _profile.PixelsPerUnit) > 0.01f) result.AddWarning("RPG_SPRITESHEET_PPU_MISMATCH", $"{name}/{frame.frameId} sprite pixels-per-unit does not match the profile.", Id);
                if (string.IsNullOrWhiteSpace(frame.frameId)) result.AddWarning("RPG_SPRITESHEET_EMPTY_FRAME_ID", $"{name} contains a frame without an id.", Id);
                else if (!ids.Add(frame.frameId)) result.AddError("RPG_SPRITESHEET_DUPLICATE_FRAME_ID", $"{name} has duplicate frame id {frame.frameId}.", Id);
                else if (!IsValidFrameId(frame.frameId)) result.AddWarning("RPG_SPRITESHEET_INVALID_FRAME_ID", $"{name}/{frame.frameId} contains characters outside stable id naming rules.", Id);
                if (!gridPositions.Add(frame.gridPosition)) result.AddError("RPG_SPRITESHEET_DUPLICATE_GRID_POSITION", $"{name} has duplicate frame grid position {frame.gridPosition}.", Id);
                if (_sourceTexture != null && !IsRectInsideTexture(frame.sourceRect, _sourceTexture.width, _sourceTexture.height)) result.AddError("RPG_SPRITESHEET_FRAME_OUT_OF_BOUNDS", $"{name}/{frame.frameId} rectangle is outside the source texture.", Id);
            }
            return result;
        }

        public static bool IsRectInsideTexture(RectInt rect, int width, int height) => rect.width > 0 && rect.height > 0 && rect.xMin >= 0 && rect.yMin >= 0 && rect.xMax <= width && rect.yMax <= height;
        private static bool IsValidFrameId(string id) => id.All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '.');
    }
}
