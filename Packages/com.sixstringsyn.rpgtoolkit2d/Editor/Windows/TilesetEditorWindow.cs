using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public static class RPGTilesetGenerator
    {
        public static List<RPGTileDefinition> GenerateTiles(RPGSpriteSheetAsset sheet, IEnumerable<RPGSpriteFrameMetadata> frames = null)
        {
            return (frames ?? sheet?.Frames ?? Array.Empty<RPGSpriteFrameMetadata>()).Where(frame => frame != null).Select(frame => new RPGTileDefinition
            {
                tileId = Sanitize(frame.frameId),
                sourceFrameId = frame.frameId,
                sprite = frame.sprite,
                blocksMovement = frame.blocksMovement || frame.defaultTileFlags.HasFlag(RPGSpriteTileFlags.BlocksMovement),
                overhead = frame.defaultTileFlags.HasFlag(RPGSpriteTileFlags.Overhead),
                terrainTags = frame.tags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>(),
                terrainTag = frame.tags?.FirstOrDefault(tag => !string.IsNullOrWhiteSpace(tag)),
                category = GuessCategory(frame),
                collisionShapeMode = frame.blocksMovement || frame.defaultTileFlags.HasFlag(RPGSpriteTileFlags.BlocksMovement) ? RPGTileCollisionShapeMode.Full : RPGTileCollisionShapeMode.None,
                defaultLayerKind = frame.defaultTileFlags.HasFlag(RPGSpriteTileFlags.Overhead) ? RPGTileLayerKind.Overhead : RPGTileLayerKind.Ground
            }).ToList();
        }

        public static RPGTilePaletteDefinition GeneratePalette(string paletteId, string displayName, IEnumerable<RPGTileDefinition> tiles, params string[] filterTags)
        {
            var filters = filterTags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList() ?? new List<string>();
            var ordered = (tiles ?? Array.Empty<RPGTileDefinition>()).Where(tile => tile != null && (filters.Count == 0 || tile.AllTerrainTags().Any(tag => filters.Contains(tag, StringComparer.OrdinalIgnoreCase)))).Select(tile => tile.tileId).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            return new RPGTilePaletteDefinition { paletteId = Sanitize(paletteId), displayName = string.IsNullOrWhiteSpace(displayName) ? paletteId : displayName, categoryFilterTags = filters, orderedTileIds = ordered };
        }

        private static RPGTileCategory GuessCategory(RPGSpriteFrameMetadata frame)
        {
            if (frame.tags == null) return RPGTileCategory.Terrain;
            if (frame.tags.Any(tag => string.Equals(tag, "water", StringComparison.OrdinalIgnoreCase) || string.Equals(tag, "liquid", StringComparison.OrdinalIgnoreCase))) return RPGTileCategory.Water;
            if (frame.tags.Any(tag => string.Equals(tag, "wall", StringComparison.OrdinalIgnoreCase))) return RPGTileCategory.Wall;
            if (frame.tags.Any(tag => string.Equals(tag, "decoration", StringComparison.OrdinalIgnoreCase) || string.Equals(tag, "decor", StringComparison.OrdinalIgnoreCase))) return RPGTileCategory.Decoration;
            return RPGTileCategory.Terrain;
        }

        private static string Sanitize(string value) => string.IsNullOrWhiteSpace(value) ? "tile" : new string(value.Trim().Select(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '.' ? ch : '_').ToArray()).ToLowerInvariant();
    }

    public sealed class TilesetEditorWindow : EditorWindow
    {
        private RPGTilesetDefinition _tileset;
        private RPGSpriteSheetAsset _sheet;
        private Vector2 _scroll;
        private string _paletteId = "default";
        private string _paletteName = "Default";

        [MenuItem("Tools/RPG Toolkit/Maps/Tileset Editor")]
        public static void Open() => GetWindow<TilesetEditorWindow>("Tilesets");

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Tileset Authoring", EditorStyles.boldLabel);
            _tileset = (RPGTilesetDefinition)EditorGUILayout.ObjectField("Tileset", _tileset, typeof(RPGTilesetDefinition), false);
            _sheet = (RPGSpriteSheetAsset)EditorGUILayout.ObjectField("Sprite Sheet", _sheet, typeof(RPGSpriteSheetAsset), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Tileset")) CreateTileset();
                if (GUILayout.Button("Generate Tiles From Frames")) GenerateTiles();
                if (GUILayout.Button("Create Palette")) CreatePalette();
            }
            _paletteId = EditorGUILayout.TextField("Palette ID", _paletteId);
            _paletteName = EditorGUILayout.TextField("Palette Name", _paletteName);
            if (_tileset != null) DrawTilesetSummary();
            EditorGUILayout.EndScrollView();
        }

        private void CreateTileset()
        {
            var directory = _sheet == null ? "Assets" : Path.GetDirectoryName(AssetDatabase.GetAssetPath(_sheet));
            _tileset = CreateInstance<RPGTilesetDefinition>();
            _tileset.Configure(_sheet);
            AssetDatabase.CreateAsset(_tileset, AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, (_sheet == null ? "NewRPGTileset" : _sheet.name + "Tileset") + ".asset")));
            Selection.activeObject = _tileset;
        }

        private void GenerateTiles()
        {
            if (_tileset == null || _sheet == null) return;
            Undo.RecordObject(_tileset, "Generate Tileset Tiles");
            _tileset.Configure(_sheet);
            _tileset.SetTiles(RPGTilesetGenerator.GenerateTiles(_sheet));
            EditorUtility.SetDirty(_tileset);
            AssetDatabase.SaveAssets();
        }

        private void CreatePalette()
        {
            if (_tileset == null) return;
            Undo.RecordObject(_tileset, "Create Tileset Palette");
            var palettes = _tileset.Palettes.ToList();
            palettes.Add(RPGTilesetGenerator.GeneratePalette(_paletteId, _paletteName, _tileset.Tiles));
            _tileset.SetPalettes(palettes);
            EditorUtility.SetDirty(_tileset);
            AssetDatabase.SaveAssets();
        }

        private void DrawTilesetSummary()
        {
            var validation = _tileset.ValidateTileset();
            EditorGUILayout.LabelField($"Tiles: {_tileset.Tiles.Count} | Palettes: {_tileset.Palettes.Count} | Valid: {validation.IsValid}");
            foreach (var message in validation.Messages) EditorGUILayout.HelpBox($"{message.Code}: {message.Message}", message.IsError ? MessageType.Error : MessageType.Warning);
        }
    }
}
