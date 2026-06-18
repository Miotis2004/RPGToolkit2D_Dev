using System;
using System.Collections.Generic;
using System.IO;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public static class RPGSpriteSheetMetadataGenerator
    {
        public static List<RPGSpriteFrameMetadata> GenerateGridFrames(Texture2D texture, RPGSpriteSheetProfile profile, string sheetName)
        {
            var frames = new List<RPGSpriteFrameMetadata>();
            if (texture == null || profile == null || profile.CellSize.x <= 0 || profile.CellSize.y <= 0) return frames;
            var stepX = profile.CellSize.x + Math.Max(0, profile.Spacing.x);
            var stepY = profile.CellSize.y + Math.Max(0, profile.Spacing.y);
            var columns = Math.Max(0, (texture.width - profile.Margin.x * 2 + Math.Max(0, profile.Spacing.x)) / stepX);
            var rows = Math.Max(0, (texture.height - profile.Margin.y * 2 + Math.Max(0, profile.Spacing.y)) / stepY);
            var index = 0;
            for (var primary = 0; primary < (profile.Ordering == RPGSpriteSheetOrdering.RowMajor ? rows : columns); primary++)
            for (var secondary = 0; secondary < (profile.Ordering == RPGSpriteSheetOrdering.RowMajor ? columns : rows); secondary++)
            {
                var x = profile.Ordering == RPGSpriteSheetOrdering.RowMajor ? secondary : primary;
                var y = profile.Ordering == RPGSpriteSheetOrdering.RowMajor ? primary : secondary;
                var originX = profile.Orientation == RPGSpriteSheetOrientation.TopRight || profile.Orientation == RPGSpriteSheetOrientation.BottomRight ? columns - 1 - x : x;
                var originY = profile.Orientation == RPGSpriteSheetOrientation.BottomLeft || profile.Orientation == RPGSpriteSheetOrientation.BottomRight ? y : rows - 1 - y;
                var rect = new RectInt(profile.Margin.x + originX * stepX, profile.Margin.y + originY * stepY, profile.CellSize.x, profile.CellSize.y);
                var frameId = FormatFrameId(profile.DefaultFrameNamingPattern, sheetName, index, x, y);
                frames.Add(new RPGSpriteFrameMetadata
                {
                    frameId = frameId,
                    gridPosition = new Vector2Int(x, y),
                    sourceRect = rect,
                    normalizedUv = new Rect(rect.x / (float)texture.width, rect.y / (float)texture.height, rect.width / (float)texture.width, rect.height / (float)texture.height),
                    pivot = profile.Pivot,
                    blocksMovement = profile.DefaultBlocksMovement,
                    defaultTileFlags = profile.DefaultBlocksMovement ? RPGSpriteTileFlags.BlocksMovement : RPGSpriteTileFlags.None,
                    tags = new List<string>(profile.DefaultTags),
                    addressKey = $"{sheetName}/{frameId}"
                });
                index++;
            }
            return frames;
        }

        private static string FormatFrameId(string pattern, string sheet, int index, int x, int y) => (pattern ?? "{sheet}_{index}").Replace("{sheet}", Sanitize(sheet)).Replace("{index}", index.ToString("000")).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
        private static string Sanitize(string value) => string.IsNullOrWhiteSpace(value) ? "sheet" : value.Replace(' ', '_').ToLowerInvariant();
    }

    public sealed class SpriteSheetEditorWindow : EditorWindow
    {
        private Texture2D _texture;
        private RPGSpriteSheetProfile _profile;
        private RPGSpriteSheetAsset _sheet;
        private Vector2 _scroll;

        [MenuItem("Tools/RPG Toolkit/Maps/Sprite Sheet Editor")]
        public static void Open() => GetWindow<SpriteSheetEditorWindow>("Sprite Sheets");

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Sprite Sheet Import", EditorStyles.boldLabel);
            _texture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", _texture, typeof(Texture2D), false);
            _profile = (RPGSpriteSheetProfile)EditorGUILayout.ObjectField("Profile", _profile, typeof(RPGSpriteSheetProfile), false);
            _sheet = (RPGSpriteSheetAsset)EditorGUILayout.ObjectField("Sheet Asset", _sheet, typeof(RPGSpriteSheetAsset), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Profile")) ProjectWindowUtil.CreateAsset(CreateInstance<RPGSpriteSheetProfile>(), "NewRPGSpriteSheetProfile.asset");
                if (GUILayout.Button("Create Sheet From Texture")) CreateSheetFromTexture();
                if (GUILayout.Button("Generate Metadata")) GenerateMetadata();
            }

            DrawImportWarnings();
            if (_sheet != null)
            {
                var result = _sheet.ValidateSpriteSheet();
                EditorGUILayout.LabelField($"Frames: {_sheet.Frames.Count} | Valid: {result.IsValid}");
                foreach (var message in result.Messages) EditorGUILayout.HelpBox($"{message.Code}: {message.Message}", message.IsError ? MessageType.Error : MessageType.Warning);
            }
            if (_texture != null) GUILayout.Label(_texture, GUILayout.MaxHeight(256));
            EditorGUILayout.EndScrollView();
        }

        private void CreateSheetFromTexture()
        {
            if (_texture == null) return;
            var path = AssetDatabase.GetAssetPath(_texture);
            var directory = string.IsNullOrEmpty(path) ? "Assets" : Path.GetDirectoryName(path);
            _sheet = CreateInstance<RPGSpriteSheetAsset>();
            _sheet.Configure(_texture, _profile);
            AssetDatabase.CreateAsset(_sheet, AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, _texture.name + "SpriteSheet.asset")));
            Selection.activeObject = _sheet;
        }

        private void GenerateMetadata()
        {
            if (_sheet == null || _texture == null || _profile == null) return;
            Undo.RecordObject(_sheet, "Generate Sprite Sheet Metadata");
            _sheet.Configure(_texture, _profile);
            _sheet.SetFrames(RPGSpriteSheetMetadataGenerator.GenerateGridFrames(_texture, _profile, _texture.name));
            EditorUtility.SetDirty(_sheet);
            AssetDatabase.SaveAssets();
        }

        private void DrawImportWarnings()
        {
            if (_texture == null) return;
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_texture)) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType != TextureImporterType.Sprite) EditorGUILayout.HelpBox("Texture type should be Sprite for sprite sheet workflows.", MessageType.Warning);
            if (_profile != null && _profile.SlicingMode == RPGSpriteSheetSlicingMode.UnitySpriteMetadata && importer.spriteImportMode != SpriteImportMode.Multiple) EditorGUILayout.HelpBox("Unity sprite metadata slicing expects Sprite Mode: Multiple.", MessageType.Warning);
        }
    }
}
