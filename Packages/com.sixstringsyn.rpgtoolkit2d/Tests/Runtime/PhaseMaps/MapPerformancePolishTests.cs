using System.Linq;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.PhaseMaps
{
    public sealed class MapPerformancePolishTests
    {
        [Test]
        public void RuntimeQueryCacheSupportsLargeMapTileObjectAndZoneLookups()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            map.Configure(new Vector2Int(128, 128));
            var layer = map.AddLayer("Ground", RPGMapLayerKind.Ground);

            for (var y = 0; y < 64; y++)
            for (var x = 0; x < 64; x++)
                map.PaintTile(layer.layerId, new Vector2Int(x, y), (x + y) % 2 == 0 ? "grass" : "water");

            var zone = map.AddZone("Encounter", RPGMapZoneKind.Encounter, new RectInt(10, 10, 20, 20), "slime-pack");
            zone.tags.Add("forest");
            var blocker = map.PlaceObject("Boulder", RPGMapObjectCategory.Decoration, new Vector2Int(12, 12));
            blocker.blocksMovement = true;
            map.MarkRuntimeQueryCacheDirty();

            map.BuildRuntimeQueryCache();

            Assert.AreEqual("grass", map.GetLayerTile(layer.layerId, new Vector2Int(2, 2)).tileId);
            Assert.AreEqual("slime-pack", map.GetHighestPriorityZoneAt(new Vector2Int(12, 12), RPGMapZoneKind.Encounter, "forest").payloadId);
            Assert.AreEqual("Boulder", map.GetObjectsInRegion(new RectInt(12, 12, 1, 1)).Single().displayName);
            Assert.IsTrue(map.IsBlocked(new Vector2Int(12, 12)));
        }

        [Test]
        public void StampAssetsReuseMultiTileBrushesAcrossMaps()
        {
            var stamp = ScriptableObject.CreateInstance<RPGMapStampDefinition>();
            stamp.Configure(new Vector2Int(2, 1), new[]
            {
                new RPGMapStampPlacement { offset = Vector2Int.zero, tileId = "left" },
                new RPGMapStampPlacement { offset = Vector2Int.right, tileId = "right", flipX = true }
            }, new[] { "roads" });

            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            map.Configure(new Vector2Int(8, 8));
            var layer = map.AddLayer("Ground", RPGMapLayerKind.Ground);

            Assert.AreEqual(2, map.PaintStamp(layer.layerId, stamp, new Vector2Int(3, 4)));
            Assert.AreEqual("left", map.GetLayerTile(layer.layerId, new Vector2Int(3, 4)).tileId);
            Assert.IsTrue(map.GetLayerTile(layer.layerId, new Vector2Int(4, 4)).flipX);
            CollectionAssert.Contains(stamp.PaletteTags.ToArray(), "roads");
        }
    }
}
