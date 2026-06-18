using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Data
{
    [Serializable]
    public sealed class RPGSpriteFrameMetadata
    {
        public string frameId;
        public Sprite sprite;
        public Vector2Int gridPosition;
        public Vector2 pivot = new Vector2(0.5f, 0f);
        public bool blocksMovement;
        public string animationKey;
        public int animationFrame;
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

        public RPGValidationResult ValidateSpriteSheet()
        {
            var result = new RPGValidationResult();
            if (_sourceTexture == null) result.AddError("RPG_SPRITESHEET_MISSING_TEXTURE", $"{name} has no source texture.", Id);
            if (_profile == null) result.AddError("RPG_SPRITESHEET_MISSING_PROFILE", $"{name} has no slicing profile.", Id);
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var frame in _frames)
            {
                if (frame == null) { result.AddWarning("RPG_SPRITESHEET_NULL_FRAME", $"{name} contains an empty frame.", Id); continue; }
                if (frame.sprite == null) result.AddWarning("RPG_SPRITESHEET_MISSING_SPRITE", $"{name}/{frame.frameId} has no sprite assigned.", Id);
                if (string.IsNullOrWhiteSpace(frame.frameId)) result.AddWarning("RPG_SPRITESHEET_EMPTY_FRAME_ID", $"{name} contains a frame without an id.", Id);
                else if (!ids.Add(frame.frameId)) result.AddError("RPG_SPRITESHEET_DUPLICATE_FRAME_ID", $"{name} has duplicate frame id {frame.frameId}.", Id);
            }
            return result;
        }
    }
}
