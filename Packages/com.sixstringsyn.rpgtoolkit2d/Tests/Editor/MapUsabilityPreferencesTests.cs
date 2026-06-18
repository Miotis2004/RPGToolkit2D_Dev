using System.Linq;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor.Windows;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor
{
    public sealed class MapUsabilityPreferencesTests
    {
        [Test]
        public void PaletteSearchMatchesTileIdsCategoriesTagsAndMetadata()
        {
            var tileset = UnityEngine.ScriptableObject.CreateInstance<RPGTilesetDefinition>();
            tileset.SetTiles(new[]
            {
                new RPGTileDefinition { tileId = "grass_corner", category = RPGTileCategory.Terrain, terrainTags = { "forest" } },
                new RPGTileDefinition { tileId = "stone_wall", category = RPGTileCategory.Wall, customMetadata = { new RPGTileCustomMetadata { key = "biome", value = "dungeon" } } }
            });

            Assert.AreEqual("grass_corner", MapUsabilityPreferences.SearchPalette(tileset, "forest").Single().tileId);
            Assert.AreEqual("stone_wall", MapUsabilityPreferences.SearchPalette(tileset, "dungeon").Single().tileId);
            Assert.AreEqual(2, MapUsabilityPreferences.SearchPalette(tileset, string.Empty).Count());
        }
    }
}
