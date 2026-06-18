using System.Collections;
using System.Linq;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.PhaseMaps
{
    public sealed class MapDataModelExpansionTests
    {
        [Test]
        public void MigratedLayersKeepLegacyTilesQueryable()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            var layers = GetList(map, "_layers");
            layers.Add(new RPGMapLayer { layerId = string.Empty, kind = RPGMapLayerKind.Ground, tiles = { new RPGMapTile { tileId = "grass", position = new Vector2Int(2, 3) } } });

            map.MigrateExpandedMapData();

            Assert.AreEqual("Layer_1", map.Layers[0].layerId);
            Assert.AreEqual("Layer_1", map.Layers[0].displayName);
            Assert.AreEqual("grass", map.GetTopTile(new Vector2Int(2, 3)).tileId);
            Assert.AreEqual("grass", map.GetTilesAt(new Vector2Int(2, 3)).Single().tileId);
        }

        [Test]
        public void LayerTileAndObjectRegionQueriesReturnExpectedContent()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            GetList(map, "_layers").Add(new RPGMapLayer
            {
                layerId = "ground",
                renderOrder = 10,
                tiles = { new RPGMapTile { tileId = "grass", position = new Vector2Int(1, 1), flipX = true } }
            });
            GetList(map, "_objects").Add(new RPGMapObjectPlacement
            {
                objectId = "chest-1",
                category = RPGMapObjectCategory.Chest,
                gridPosition = new Vector2Int(2, 2),
                blocksMovement = true
            });

            Assert.IsTrue(map.GetLayerTile("ground", new Vector2Int(1, 1)).flipX);
            Assert.AreEqual("chest-1", map.GetObjectsInRegion(new RectInt(0, 0, 4, 4), RPGMapObjectCategory.Chest).Single().objectId);
            Assert.IsTrue(map.IsBlocked(new Vector2Int(2, 2)));
        }

        [Test]
        public void ZonesCanBeQueriedByKindAndTag()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            GetList(map, "_zones").Add(new RPGMapZone { zoneId = "forest", kind = RPGMapZoneKind.Encounter, bounds = new RectInt(0, 0, 4, 4), priority = 3, tags = { "night" } });
            GetList(map, "_zones").Add(new RPGMapZone { zoneId = "lights", kind = RPGMapZoneKind.Lighting, bounds = new RectInt(0, 0, 4, 4), tags = { "night" } });
            GetList(map, "_zones").Add(new RPGMapZone { zoneId = "ambush", kind = RPGMapZoneKind.Encounter, bounds = new RectInt(0, 0, 4, 4), priority = 9, tags = { "night" } });

            Assert.AreEqual("ambush", map.GetZonesAt(new Vector2Int(1, 1), RPGMapZoneKind.Encounter, "night").First().zoneId);
            Assert.AreEqual("ambush", map.GetHighestPriorityZoneAt(new Vector2Int(1, 1), RPGMapZoneKind.Encounter, "night").zoneId);
        }

        [Test]
        public void EffectiveCollisionAndCellMetadataIncludeTilesZonesObjectsAndConnections()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            map.Configure(new Vector2Int(8, 8));
            var layer = map.AddLayer("Ground");
            map.PaintTile(layer.layerId, new Vector2Int(2, 2), "grass");
            map.GetLayerTile(layer.layerId, new Vector2Int(2, 2)).overrideMetadata.Add(new RPGMapMetadataEntry { key = "footstep", value = "grass" });
            map.AddZone("forest", RPGMapZoneKind.Encounter, new RectInt(1, 1, 3, 3), "slime-pack").priority = 10;
            GetList(map, "_objects").Add(new RPGMapObjectPlacement { objectId = "crate", gridPosition = new Vector2Int(2, 2), blocksMovement = true, metadata = { new RPGMapMetadataEntry { key = "loot", value = "potion" } } });
            GetList(map, "_exits").Add(new RPGMapExit { exitId = "north", position = new Vector2Int(2, 2), targetMapId = "overworld", targetEntranceId = "south" });

            var metadata = map.GetCellMetadata(new Vector2Int(2, 2));

            Assert.IsTrue(metadata.blocked);
            Assert.AreEqual("grass", metadata.tiles.Single().tileId);
            Assert.AreEqual("forest", metadata.zones.Single().zoneId);
            Assert.AreEqual("crate", metadata.objects.Single().objectId);
            CollectionAssert.Contains(metadata.metadata.Select(entry => entry.key).ToArray(), "payloadId");
            CollectionAssert.Contains(metadata.metadata.Select(entry => entry.key).ToArray(), "footstep");
            CollectionAssert.Contains(metadata.metadata.Select(entry => entry.key).ToArray(), "loot");
        }

        [Test]
        public void EntrancesExitsAndConnectionsResolveById()
        {
            var target = ScriptableObject.CreateInstance<RPGMapDefinition>();
            GetList(target, "_entrances").Add(new RPGMapEntrance { entranceId = "south", position = new Vector2Int(1, 1) });
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            GetList(map, "_exits").Add(new RPGMapExit { exitId = "north-road", position = new Vector2Int(0, 0), targetMap = target, targetEntranceId = "south" });
            GetList(map, "_connections").Add(new RPGMapConnection { connectionId = "overworld", targetMap = target, targetEntranceId = "south" });

            Assert.AreEqual("south", target.ResolveEntrance("south").entranceId);
            Assert.AreEqual("north-road", map.ResolveExit("north-road").exitId);
            Assert.AreEqual("overworld", map.ResolveConnection("overworld").connectionId);
        }


        [Test]
        public void ObjectPlacementOperationsProduceSpawnDescriptors()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            map.Configure(new Vector2Int(8, 8));

            var chest = map.PlaceObject("Starter Chest", RPGMapObjectCategory.Chest, new Vector2Int(2, 3));
            chest.addressableKey = "objects/chest";
            chest.worldOffset = new Vector3(0.25f, 0.5f, 0f);
            chest.rotationEuler = new Vector3(0f, 0f, 90f);
            chest.persistentStateKey = "chests.starter";
            chest.metadata.Add(new RPGMapMetadataEntry { key = "loot", value = "potion" });

            Assert.IsTrue(map.MoveObject(chest.objectId, new Vector2Int(4, 5), new Vector3(0.5f, 0f, 0f)));
            Assert.IsTrue(map.RotateObject(chest.objectId, new Vector3(0f, 0f, 180f)));
            var copy = map.DuplicateObject(chest.objectId, new Vector2Int(1, 1));

            var descriptors = RPGMapObjectSpawner.BuildSpawnDescriptors(map).ToArray();

            Assert.AreEqual(2, descriptors.Length);
            Assert.AreEqual("Starter Chest", descriptors.First(descriptor => descriptor.objectId == chest.objectId).displayName);
            Assert.AreEqual(new Vector3(4.5f, 5f, 0f), descriptors.First(descriptor => descriptor.objectId == chest.objectId).worldPosition);
            Assert.AreNotEqual(chest.objectId, copy.objectId);
            Assert.IsTrue(map.RemoveObject(copy.objectId));
        }

        [Test]
        public void ExitConnectionResolutionRequiresTargetEntranceWhenSpecified()
        {
            var target = ScriptableObject.CreateInstance<RPGMapDefinition>();
            target.Configure(new Vector2Int(4, 4));
            target.AddEntrance("south gate", new Vector2Int(1, 0));

            var source = ScriptableObject.CreateInstance<RPGMapDefinition>();
            source.Configure(new Vector2Int(4, 4));
            var exit = source.AddExit("north gate", new Vector2Int(1, 3), target, "south_gate", RPGMapTransitionKind.Door);

            var resolution = source.ResolveExitConnection(exit.exitId);

            Assert.IsTrue(resolution.IsResolved);
            Assert.AreSame(target, resolution.TargetMap);
            Assert.AreEqual("south_gate", resolution.TargetEntrance.entranceId);
        }

        [Test]
        public void ValidationReportsExpandedMapIssues()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            GetList(map, "_layers").Add(new RPGMapLayer { layerId = "duplicate", renderOrder = 0, tiles = { new RPGMapTile { tileId = "missing", position = new Vector2Int(99, 99), overrideMetadata = { new RPGMapMetadataEntry { key = "bad key" } } } } });
            GetList(map, "_layers").Add(new RPGMapLayer { layerId = "duplicate", renderOrder = 0 });
            GetList(map, "_objects").Add(new RPGMapObjectPlacement { objectId = "crate", gridPosition = new Vector2Int(40, 40) });
            GetList(map, "_objects").Add(new RPGMapObjectPlacement { objectId = "crate", gridPosition = new Vector2Int(1, 1) });
            GetList(map, "_entrances").Add(new RPGMapEntrance { entranceId = "door", position = new Vector2Int(1, 1) });
            GetList(map, "_entrances").Add(new RPGMapEntrance { entranceId = "door", position = new Vector2Int(2, 2) });
            GetList(map, "_exits").Add(new RPGMapExit { exitId = "exit", position = new Vector2Int(1, 1) });
            GetList(map, "_exits").Add(new RPGMapExit { exitId = "exit", position = new Vector2Int(2, 2), targetMapId = "other" });
            GetList(map, "_zones").Add(new RPGMapZone { zoneId = "bad-zone", bounds = new RectInt(31, 31, 3, 3) });

            var codes = map.ValidateMap().Messages.Select(message => message.Code).ToArray();

            CollectionAssert.Contains(codes, "RPG_MAP_DUPLICATE_LAYER_ID");
            CollectionAssert.Contains(codes, "RPG_MAP_TILE_OUT_OF_BOUNDS");
            CollectionAssert.Contains(codes, "RPG_MAP_INVALID_TILE_METADATA_KEY");
            CollectionAssert.Contains(codes, "RPG_MAP_DUPLICATE_OBJECT_ID");
            CollectionAssert.Contains(codes, "RPG_MAP_OBJECT_OUT_OF_BOUNDS");
            CollectionAssert.Contains(codes, "RPG_MAP_DUPLICATE_ENTRANCE_ID");
            CollectionAssert.Contains(codes, "RPG_MAP_DUPLICATE_EXIT_ID");
            CollectionAssert.Contains(codes, "RPG_MAP_EXIT_MISSING_TARGET");
            CollectionAssert.Contains(codes, "RPG_MAP_ZONE_OUT_OF_BOUNDS");
        }

        private static IList GetList(RPGMapDefinition map, string fieldName) => (IList)typeof(RPGMapDefinition).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(map);
    }
}
