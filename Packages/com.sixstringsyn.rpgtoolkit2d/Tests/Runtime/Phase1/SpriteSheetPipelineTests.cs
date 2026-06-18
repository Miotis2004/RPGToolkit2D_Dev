using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Phase1
{
    public sealed class SpriteSheetPipelineTests
    {
        [Test]
        public void SpriteSheetFramesCanBeFoundByIdTagGroupAndContentKind()
        {
            var profile = ScriptableObject.CreateInstance<RPGSpriteSheetProfile>();
            var sheet = ScriptableObject.CreateInstance<RPGSpriteSheetAsset>();
            sheet.Configure(new Texture2D(32, 16), profile);
            sheet.SetFrames(new[]
            {
                new RPGSpriteFrameMetadata { frameId = "grass", gridPosition = Vector2Int.zero, sourceRect = new RectInt(0, 0, 16, 16), tags = new List<string> { "terrain" }, frameGroupId = "field" },
                new RPGSpriteFrameMetadata { frameId = "water", gridPosition = new Vector2Int(1, 0), sourceRect = new RectInt(16, 0, 16, 16), tags = new List<string> { "terrain", "liquid" }, frameGroupId = "field" }
            });

            Assert.AreEqual("grass", sheet.FindFrame("GRASS").frameId);
            Assert.AreEqual(2, sheet.GetFramesByTag("terrain").Count());
            Assert.AreEqual(2, sheet.GetFramesByGroup("field").Count());
            Assert.AreEqual(2, sheet.GetFramesByContentKind(RPGContentKind.SpriteSheet).Count());
        }

        [Test]
        public void SpriteSheetValidationReportsDuplicateGridAndOutOfBoundsFrames()
        {
            var profile = ScriptableObject.CreateInstance<RPGSpriteSheetProfile>();
            var sheet = ScriptableObject.CreateInstance<RPGSpriteSheetAsset>();
            sheet.name = "Tiles";
            sheet.Configure(new Texture2D(16, 16), profile);
            sheet.SetFrames(new[]
            {
                new RPGSpriteFrameMetadata { frameId = "tile", gridPosition = Vector2Int.zero, sourceRect = new RectInt(0, 0, 16, 16) },
                new RPGSpriteFrameMetadata { frameId = "tile", gridPosition = Vector2Int.zero, sourceRect = new RectInt(16, 0, 16, 16) }
            });

            var validation = sheet.ValidateSpriteSheet();

            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_SPRITESHEET_DUPLICATE_FRAME_ID"));
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_SPRITESHEET_DUPLICATE_GRID_POSITION"));
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_SPRITESHEET_FRAME_OUT_OF_BOUNDS"));
        }
    }
}
