using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor
{
    public sealed class MapEditorCoreTests
    {
        [Test]
        public void MapCreationLayerAndSaveOperationsPersistEditorChanges()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            map.Configure(new Vector2Int(8, 8));

            Undo.RecordObject(map, "Add Map Layer");
            var layer = map.AddLayer("Ground", RPGMapLayerKind.Ground);
            EditorUtility.SetDirty(map);

            Assert.AreEqual("Ground", layer.displayName);
            Assert.AreSame(layer, map.FindLayer(layer.layerId));
            Assert.IsTrue(EditorUtility.IsDirty(map));
        }

        [Test]
        public void BrushOperationsPaintEraseFillFloodAndReplaceSerializedMapData()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            map.Configure(new Vector2Int(5, 5));
            var layer = map.AddLayer("Ground", RPGMapLayerKind.Ground);

            Assert.IsTrue(map.PaintTile(layer.layerId, new Vector2Int(1, 1), "grass"));
            Assert.AreEqual("grass", map.GetLayerTile(layer.layerId, new Vector2Int(1, 1)).tileId);

            Assert.AreEqual(4, map.FillRectangle(layer.layerId, new RectInt(0, 0, 2, 2), "water"));
            Assert.AreEqual("water", map.GetLayerTile(layer.layerId, Vector2Int.zero).tileId);

            Assert.GreaterOrEqual(map.FloodFill(layer.layerId, new Vector2Int(3, 3), "sand"), 1);
            Assert.AreEqual("sand", map.GetLayerTile(layer.layerId, new Vector2Int(3, 3)).tileId);

            Assert.AreEqual(4, map.ReplaceTile(layer.layerId, "water", "stone"));
            Assert.AreEqual("stone", map.GetLayerTile(layer.layerId, Vector2Int.zero).tileId);

            Assert.IsTrue(map.EraseTile(layer.layerId, Vector2Int.zero));
            Assert.IsNull(map.GetLayerTile(layer.layerId, Vector2Int.zero));
        }

        [Test]
        public void UndoRedoRestoresPaintOperationsWherePractical()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            map.Configure(new Vector2Int(4, 4));
            var layer = map.AddLayer("Ground", RPGMapLayerKind.Ground);

            Undo.IncrementCurrentGroup();
            Undo.RecordObject(map, "Paint Tile");
            map.PaintTile(layer.layerId, new Vector2Int(2, 2), "grass");
            Assert.IsNotNull(map.GetLayerTile(layer.layerId, new Vector2Int(2, 2)));

            Undo.PerformUndo();
            Assert.IsNull(map.GetLayerTile(layer.layerId, new Vector2Int(2, 2)));

            Undo.PerformRedo();
            Assert.IsNotNull(map.GetLayerTile(layer.layerId, new Vector2Int(2, 2)));
        }
    }
}
