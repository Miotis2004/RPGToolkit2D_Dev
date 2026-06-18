using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.PhaseMaps
{
    public sealed class RuntimeMapLoaderTests
    {
        [Test]
        public void LoadMapCreatesGridTilemapsAndGeneratedColliders()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            var tileset = ScriptableObject.CreateInstance<RPGTilesetDefinition>();
            tileset.SetTiles(new[] { new RPGTileDefinition { tileId = "wall", blocksMovement = true } });
            map.Configure(new Vector2Int(4, 4), tileset);
            var layer = map.AddLayer("Ground");
            map.PaintTile(layer.layerId, new Vector2Int(1, 2), "wall");

            var host = new GameObject("loader-host");
            var loader = host.AddComponent<RPGMapLoader>();
            loader.CollisionMode = RPGMapCollisionMode.GeneratedColliderObjects;

            var loaded = loader.LoadMap(map);

            Assert.NotNull(loaded.Grid);
            Assert.IsTrue(loaded.TilemapsByLayerId.ContainsKey(layer.layerId));
            Assert.NotNull(loaded.TilemapsByLayerId[layer.layerId].GetTile(new Vector3Int(1, 2, 0)));
            Assert.AreEqual(1, loaded.GeneratedColliders.Count);

            Object.DestroyImmediate(host);
        }

        [Test]
        public void LoadMapPositionsPlayerAtEntranceAndCanTransition()
        {
            var source = ScriptableObject.CreateInstance<RPGMapDefinition>();
            source.Configure(new Vector2Int(4, 4), ScriptableObject.CreateInstance<RPGTilesetDefinition>());
            var target = ScriptableObject.CreateInstance<RPGMapDefinition>();
            target.Configure(new Vector2Int(4, 4), ScriptableObject.CreateInstance<RPGTilesetDefinition>());
            target.AddEntrance("south", new Vector2Int(2, 1));
            source.AddExit("north", new Vector2Int(1, 3), target, "south");

            var player = new GameObject("player");
            var host = new GameObject("loader-host");
            var loader = host.AddComponent<RPGMapLoader>();
            typeof(RPGMapLoader).GetField("_player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(loader, player.transform);

            loader.LoadMap(source);
            Assert.IsTrue(loader.TryTransition("north"));

            Assert.AreSame(target, loader.LoadedMap.Map);
            Assert.AreEqual(new Vector3(2.5f, 1.5f, 0f), player.transform.position);

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(host);
        }
    }
}
