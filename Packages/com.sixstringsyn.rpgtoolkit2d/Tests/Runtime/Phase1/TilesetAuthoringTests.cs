using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Phase1
{
    public sealed class TilesetAuthoringTests
    {
        [Test]
        public void TilesetFindsTilesByIdCategoryTagAndSourceFrame()
        {
            var sheet = ScriptableObject.CreateInstance<RPGSpriteSheetAsset>();
            sheet.SetFrames(new[] { new RPGSpriteFrameMetadata { frameId = "grass", tags = new List<string> { "field" } } });
            var tileset = ScriptableObject.CreateInstance<RPGTilesetDefinition>();
            tileset.Configure(sheet);
            tileset.SetTiles(new[] { new RPGTileDefinition { tileId = "grass", sourceFrameId = "grass", category = RPGTileCategory.Terrain, terrainTags = new List<string> { "field" } } });

            Assert.AreEqual("grass", tileset.FindTile("GRASS").tileId);
            Assert.AreEqual(1, tileset.FindTilesByCategory(RPGTileCategory.Terrain).Count());
            Assert.AreEqual(1, tileset.FindTilesByTag("field").Count());
            Assert.AreEqual("grass", tileset.GetSourceFrame(tileset.FindTile("grass")).frameId);
        }

        [Test]
        public void TilesetValidationReportsUnknownFramesDuplicateTilesAndInvalidPalettes()
        {
            var sheet = ScriptableObject.CreateInstance<RPGSpriteSheetAsset>();
            sheet.SetFrames(new[] { new RPGSpriteFrameMetadata { frameId = "known" } });
            var tileset = ScriptableObject.CreateInstance<RPGTilesetDefinition>();
            tileset.name = "Dungeon";
            tileset.Configure(sheet);
            tileset.SetTiles(new[]
            {
                new RPGTileDefinition { tileId = "stone", sourceFrameId = "missing", animation = new RPGTileAnimationMetadata { enabled = true } },
                new RPGTileDefinition { tileId = "stone", sourceFrameId = "known", autotile = new RPGTileAutotileMetadata { enabled = true } }
            });
            tileset.SetPalettes(new[] { new RPGTilePaletteDefinition { paletteId = "default", orderedTileIds = new List<string> { "unknown" } },
                new RPGTilePaletteDefinition { paletteId = "default" } });

            var validation = tileset.ValidateTileset();

            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_TILESET_DUPLICATE_TILE_ID"));
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_TILESET_UNKNOWN_FRAME_ID"));
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_TILESET_INVALID_PALETTE_TILE"));
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_TILESET_DUPLICATE_PALETTE_ID"));
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_TILESET_ANIMATION_MISSING_FRAMES"));
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_TILESET_AUTOTILE_INCOMPLETE"));
        }
    }
}
