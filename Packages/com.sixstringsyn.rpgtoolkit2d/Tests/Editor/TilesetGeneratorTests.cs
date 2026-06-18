using System.Collections.Generic;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor.Windows;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor
{
    public sealed class TilesetGeneratorTests
    {
        [Test]
        public void TilesetGeneratorCreatesTilesFromSpriteSheetFrames()
        {
            var sheet = ScriptableObject.CreateInstance<RPGSpriteSheetAsset>();
            sheet.SetFrames(new[]
            {
                new RPGSpriteFrameMetadata { frameId = "Grass Tile", tags = new List<string> { "field" }, defaultTileFlags = RPGSpriteTileFlags.BlocksMovement },
                new RPGSpriteFrameMetadata { frameId = "water", tags = new List<string> { "water" } }
            });

            var tiles = RPGTilesetGenerator.GenerateTiles(sheet);

            Assert.AreEqual(2, tiles.Count);
            Assert.AreEqual("grass_tile", tiles[0].tileId);
            Assert.AreEqual("Grass Tile", tiles[0].sourceFrameId);
            Assert.IsTrue(tiles[0].blocksMovement);
            Assert.AreEqual(RPGTileCategory.Water, tiles[1].category);
        }

        [Test]
        public void PaletteGeneratorOrdersOnlyMatchingTags()
        {
            var tiles = new[]
            {
                new RPGTileDefinition { tileId = "grass", terrainTags = new List<string> { "field" } },
                new RPGTileDefinition { tileId = "water", terrainTags = new List<string> { "liquid" } }
            };

            var palette = RPGTilesetGenerator.GeneratePalette("Field Palette", "Field", tiles, "field");

            Assert.AreEqual("field_palette", palette.paletteId);
            Assert.AreEqual(1, palette.orderedTileIds.Count);
            Assert.AreEqual("grass", palette.orderedTileIds[0]);
        }
    }
}
