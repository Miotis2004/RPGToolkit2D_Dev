using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Cutscenes;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using SixStringSyn.RPGToolkit2D.Runtime.Schedules;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public enum RPGMapEditorTool { Select, Pencil, Erase, RectangleFill, FloodFill, ReplaceTile, Stamp }

    public sealed class MapEditorWindow : EditorWindow
    {
        private RPGMapDefinition _map;
        private RPGTileDefinition _selectedTile;
        private string _activeLayerId;
        private RPGMapEditorTool _tool = RPGMapEditorTool.Pencil;
        private Vector2 _scroll;
        private Vector2 _pan;
        private float _zoom = 1f;
        private Vector2Int _hoverCell = new Vector2Int(-1, -1);
        private Vector2Int _dragStart;
        private bool _dragging;
        private RPGValidationResult _validation;

        [MenuItem("Tools/RPG Toolkit/Maps/Map Editor")]
        public static void Open() => GetWindow<MapEditorWindow>("Map Editor");

        private void OnGUI()
        {
            HandleShortcuts(Event.current);
            DrawToolbar();
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLayerPanel();
                DrawCanvas();
                DrawInspectorPanel();
            }
            DrawStatusBar();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var nextMap = (RPGMapDefinition)EditorGUILayout.ObjectField(_map, typeof(RPGMapDefinition), false, GUILayout.Width(260));
                if (nextMap != _map) SelectMap(nextMap);
                if (GUILayout.Button("New Map", EditorStyles.toolbarButton, GUILayout.Width(80))) CreateMapAsset();
                _tool = (RPGMapEditorTool)EditorGUILayout.EnumPopup(_tool, EditorStyles.toolbarPopup, GUILayout.Width(120));
                GUILayout.Label("Zoom", GUILayout.Width(36));
                _zoom = GUILayout.HorizontalSlider(_zoom, 0.35f, 3f, GUILayout.Width(120));
                if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(80))) RefreshValidation();
                if (GUILayout.Button("Docs", EditorStyles.toolbarButton, GUILayout.Width(50))) Application.OpenURL("https://github.com/SixStringSyn/RPGToolkit2D/blob/main/MAPS_DEV.md#phase-4-visual-map-editor-core");
            }
        }

        private void DrawLayerPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(220)))
            {
                GUILayout.Label("Layers", EditorStyles.boldLabel);
                if (_map == null) { EditorGUILayout.HelpBox("Select or create a map to edit layers.", MessageType.Info); return; }
                if (_map.Layers.Count == 0 && GUILayout.Button("Add Ground Layer")) AddLayer();
                foreach (var layer in _map.Layers)
                {
                    if (layer == null) continue;
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        var active = _activeLayerId == layer.layerId;
                        if (GUILayout.Toggle(active, layer.displayName, GUI.skin.button)) _activeLayerId = layer.layerId;
                        layer.visible = GUILayout.Toggle(layer.visible, "V", GUILayout.Width(24));
                        layer.locked = GUILayout.Toggle(layer.locked, "L", GUILayout.Width(24));
                    }
                }
                if (GUILayout.Button("Add Layer")) AddLayer();
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Duplicate")) DuplicateLayer();
                    if (GUILayout.Button("Remove")) RemoveLayer();
                }
            }
        }

        private void DrawCanvas()
        {
            var rect = GUILayoutUtility.GetRect(400, 400, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUI.Box(rect, GUIContent.none);
            if (_map == null) { GUI.Label(rect, "Select a map to begin painting.", EditorStyles.centeredGreyMiniLabel); return; }
            var cell = 24f * _zoom;
            var origin = rect.min + _pan + new Vector2(20f, 20f);
            DrawGrid(rect, origin, cell);
            DrawTiles(rect, origin, cell);
            HandleCanvasInput(rect, origin, cell, Event.current);
            if (_hoverCell.x >= 0) DrawCell(rect, origin, cell, _hoverCell, new Color(1f, 0.85f, 0.1f, 0.35f));
            if (_selectedTile != null && rect.Contains(Event.current.mousePosition)) GUI.Label(new Rect(Event.current.mousePosition + new Vector2(16, 16), new Vector2(180, 20)), $"{_selectedTile.tileId}");
        }

        private void DrawInspectorPanel()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(280));
            GUILayout.Label("Inspector", EditorStyles.boldLabel);
            if (_map != null)
            {
                EditorGUI.BeginChangeCheck();
                var tileset = (RPGTilesetDefinition)EditorGUILayout.ObjectField("Tileset", _map.Tileset, typeof(RPGTilesetDefinition), false);
                if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(_map, "Set Map Tileset"); _map.SetTileset(tileset); SaveMap(); }
                DrawTilePalette();
                DrawLayerInspector();
                DrawValidationPanel();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawTilePalette()
        {
            GUILayout.Space(6); GUILayout.Label("Palette", EditorStyles.boldLabel);
            if (_map.Tileset == null) { EditorGUILayout.HelpBox("Assign a tileset to select paint tiles.", MessageType.Warning); return; }
            foreach (var tile in _map.Tileset.Tiles)
            {
                if (tile == null) continue;
                if (GUILayout.Toggle(_selectedTile == tile, tile.tileId, GUI.skin.button)) _selectedTile = tile;
            }
        }

        private void DrawLayerInspector()
        {
            var layer = ActiveLayer;
            if (layer == null) return;
            GUILayout.Space(6); GUILayout.Label("Active Layer", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            layer.displayName = EditorGUILayout.TextField("Name", layer.displayName);
            layer.kind = (RPGMapLayerKind)EditorGUILayout.EnumPopup("Kind", layer.kind);
            layer.opacity = EditorGUILayout.Slider("Opacity", layer.opacity, 0f, 1f);
            if (EditorGUI.EndChangeCheck()) SaveMap();
        }

        private void DrawValidationPanel()
        {
            GUILayout.Space(6); GUILayout.Label("Validation", EditorStyles.boldLabel);
            _validation ??= _map.ValidateMap();
            EditorGUILayout.LabelField($"Errors: {_validation.Messages.Count(message => message.IsError)}  Warnings: {_validation.Messages.Count(message => message.Severity == RPGValidationSeverity.Warning)}");
            foreach (var diagnostic in _validation.Messages.Where(message => message.IsError)) EditorGUILayout.HelpBox(diagnostic.Message, MessageType.Error);
            foreach (var diagnostic in _validation.Messages.Where(message => message.Severity == RPGValidationSeverity.Warning)) EditorGUILayout.HelpBox(diagnostic.Message, MessageType.Warning);
        }

        private void DrawStatusBar()
        {
            var tile = _selectedTile?.tileId ?? "None";
            var layer = ActiveLayer?.displayName ?? "None";
            var validation = _validation == null ? "Not validated" : $"{_validation.Messages.Count(message => message.IsError)} errors, {_validation.Messages.Count(message => message.Severity == RPGValidationSeverity.Warning)} warnings";
            EditorGUILayout.LabelField($"Cell: {_hoverCell} | Tile: {tile} | Layer: {layer} | Validation: {validation}", EditorStyles.helpBox);
        }

        private RPGMapLayer ActiveLayer => _map?.FindLayer(_activeLayerId) ?? _map?.Layers.FirstOrDefault();
        private void SelectMap(RPGMapDefinition map) { _map = map; _activeLayerId = map?.Layers.FirstOrDefault()?.layerId; _validation = null; }
        private void AddLayer() { Undo.RecordObject(_map, "Add Map Layer"); var layer = _map.AddLayer($"Layer {_map.Layers.Count + 1}"); _activeLayerId = layer.layerId; SaveMap(); }
        private void DuplicateLayer() { if (ActiveLayer == null) return; Undo.RecordObject(_map, "Duplicate Map Layer"); var layer = _map.DuplicateLayer(ActiveLayer.layerId); _activeLayerId = layer?.layerId; SaveMap(); }
        private void RemoveLayer() { if (ActiveLayer == null) return; Undo.RecordObject(_map, "Remove Map Layer"); _map.RemoveLayer(ActiveLayer.layerId); _activeLayerId = _map.Layers.FirstOrDefault()?.layerId; SaveMap(); }
        private void SaveMap() { EditorUtility.SetDirty(_map); _validation = null; AssetDatabase.SaveAssets(); Repaint(); }
        private void RefreshValidation() { _validation = _map?.ValidateMap(); Repaint(); }

        private void CreateMapAsset() => ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<RPGMapDefinition>(), "NewRPGMap.asset");

        private void DrawGrid(Rect rect, Vector2 origin, float cell)
        {
            Handles.BeginGUI();
            Handles.color = new Color(1, 1, 1, 0.15f);
            for (var x = 0; x <= _map.Size.x; x++) Handles.DrawLine(new Vector3(origin.x + x * cell, origin.y), new Vector3(origin.x + x * cell, origin.y + _map.Size.y * cell));
            for (var y = 0; y <= _map.Size.y; y++) Handles.DrawLine(new Vector3(origin.x, origin.y + y * cell), new Vector3(origin.x + _map.Size.x * cell, origin.y + y * cell));
            Handles.color = Color.white; Handles.DrawAAPolyLine(2f, new Vector3(origin.x, origin.y), new Vector3(origin.x + _map.Size.x * cell, origin.y), new Vector3(origin.x + _map.Size.x * cell, origin.y + _map.Size.y * cell), new Vector3(origin.x, origin.y + _map.Size.y * cell), new Vector3(origin.x, origin.y));
            Handles.EndGUI();
        }

        private void DrawTiles(Rect rect, Vector2 origin, float cell)
        {
            foreach (var layer in _map.Layers.OrderBy(l => l.renderOrder))
            {
                if (layer == null || !layer.visible || layer.tiles == null) continue;
                GUI.color = new Color(1f, 1f, 1f, layer.opacity);
                foreach (var tile in layer.tiles)
                {
                    if (tile == null) continue;
                    var def = _map.Tileset?.FindTile(tile.tileId);
                    if (def?.sprite?.texture != null) GUI.DrawTextureWithTexCoords(CellRect(origin, cell, tile.position), def.sprite.texture, TextureCoords(def.sprite), true);
                    else EditorGUI.DrawRect(CellRect(origin, cell, tile.position), layer.kind == RPGMapLayerKind.Collision ? new Color(1,0,0,.35f) : new Color(.3f,.7f,1f,.25f));
                }
                GUI.color = Color.white;
            }
        }

        private static Rect TextureCoords(Sprite sprite)
        {
            var r = sprite.textureRect;
            return new Rect(r.x / sprite.texture.width, r.y / sprite.texture.height, r.width / sprite.texture.width, r.height / sprite.texture.height);
        }

        private void HandleCanvasInput(Rect rect, Vector2 origin, float cell, Event evt)
        {
            if (!rect.Contains(evt.mousePosition)) return;
            _hoverCell = new Vector2Int(Mathf.FloorToInt((evt.mousePosition.x - origin.x) / cell), Mathf.FloorToInt((evt.mousePosition.y - origin.y) / cell));
            if (evt.type == EventType.ScrollWheel) { _zoom = Mathf.Clamp(_zoom - evt.delta.y * .03f, .35f, 3f); evt.Use(); Repaint(); }
            if (evt.button == 2 && evt.type == EventType.MouseDrag) { _pan += evt.delta; evt.Use(); Repaint(); }
            if (evt.button != 0) return;
            if (evt.type == EventType.MouseDown) { _dragStart = _hoverCell; _dragging = true; ApplyTool(_hoverCell, false); evt.Use(); }
            if (evt.type == EventType.MouseDrag && _dragging && (_tool == RPGMapEditorTool.Pencil || _tool == RPGMapEditorTool.Erase)) { ApplyTool(_hoverCell, false); evt.Use(); }
            if (evt.type == EventType.MouseUp && _dragging) { ApplyTool(_hoverCell, true); _dragging = false; evt.Use(); }
        }

        private void ApplyTool(Vector2Int cell, bool finalDrag)
        {
            if (ActiveLayer == null || cell.x < 0 || cell.y < 0 || cell.x >= _map.Size.x || cell.y >= _map.Size.y) return;
            if ((_tool == RPGMapEditorTool.Pencil || _tool == RPGMapEditorTool.RectangleFill || _tool == RPGMapEditorTool.FloodFill || _tool == RPGMapEditorTool.ReplaceTile || _tool == RPGMapEditorTool.Stamp) && _selectedTile == null) return;
            Undo.RecordObject(_map, $"Map {_tool}");
            switch (_tool)
            {
                case RPGMapEditorTool.Pencil: _map.PaintTile(ActiveLayer.layerId, cell, _selectedTile.tileId); break;
                case RPGMapEditorTool.Erase: _map.EraseTile(ActiveLayer.layerId, cell); break;
                case RPGMapEditorTool.RectangleFill: if (finalDrag) _map.FillRectangle(ActiveLayer.layerId, Rect.MinMaxRect(Mathf.Min(_dragStart.x, cell.x), Mathf.Min(_dragStart.y, cell.y), Mathf.Max(_dragStart.x, cell.x) + 1, Mathf.Max(_dragStart.y, cell.y) + 1).ToRectInt(), _selectedTile.tileId); break;
                case RPGMapEditorTool.FloodFill: if (!finalDrag) _map.FloodFill(ActiveLayer.layerId, cell, _selectedTile.tileId); break;
                case RPGMapEditorTool.ReplaceTile: if (!finalDrag) _map.ReplaceTile(ActiveLayer.layerId, _map.GetLayerTile(ActiveLayer.layerId, cell)?.tileId, _selectedTile.tileId); break;
                case RPGMapEditorTool.Stamp: _map.PaintTile(ActiveLayer.layerId, cell, _selectedTile.tileId); break;
            }
            SaveMap();
        }

        private void HandleShortcuts(Event evt)
        {
            if (evt.type != EventType.KeyDown) return;
            if (evt.keyCode == KeyCode.V) _tool = RPGMapEditorTool.Select;
            if (evt.keyCode == KeyCode.B) _tool = RPGMapEditorTool.Pencil;
            if (evt.keyCode == KeyCode.E) _tool = RPGMapEditorTool.Erase;
            if (evt.keyCode == KeyCode.G) _tool = RPGMapEditorTool.FloodFill;
            if (evt.keyCode == KeyCode.R) _tool = RPGMapEditorTool.RectangleFill;
        }

        private static Rect CellRect(Vector2 origin, float cell, Vector2Int position) => new Rect(origin.x + position.x * cell, origin.y + position.y * cell, cell, cell);
        private static void DrawCell(Rect clip, Vector2 origin, float cell, Vector2Int position, Color color) => EditorGUI.DrawRect(CellRect(origin, cell, position), color);
    }

    internal static class RectExtensions
    {
        public static RectInt ToRectInt(this Rect rect) => new RectInt(Mathf.RoundToInt(rect.xMin), Mathf.RoundToInt(rect.yMin), Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
    }

    public sealed class NPCScheduleEditorWindow : AssetPickerWindow
    {
        [MenuItem("Tools/RPG Toolkit/World/NPC Schedule Editor")]
        public static void Open() { var window = GetWindow<NPCScheduleEditorWindow>("NPC Schedules"); window.TypeName = "NPC Schedule"; }
        protected override void CreateAsset() => ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<NPCScheduleDefinition>(), "NewNPCSchedule.asset");
    }

    public sealed class CutsceneEditorWindow : AssetPickerWindow
    {
        [MenuItem("Tools/RPG Toolkit/Cutscene Editor")]
        public static void Open() { var window = GetWindow<CutsceneEditorWindow>("Cutscenes"); window.TypeName = "Cutscene"; }
        protected override void CreateAsset() => ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<RPGCutsceneDefinition>(), "NewRPGCutscene.asset");
    }
}
