using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor
{
    public enum RPGMapValidationAssetKind { SpriteSheet, Tileset, Map, MapGraph, DuplicateId }

    public sealed class RPGMapProjectValidationEntry
    {
        public RPGMapProjectValidationEntry(RPGMapValidationAssetKind kind, UnityEngine.Object asset, string assetPath, RPGValidationMessage message)
        {
            Kind = kind; Asset = asset; AssetPath = assetPath; Message = message;
        }
        public RPGMapValidationAssetKind Kind { get; }
        public UnityEngine.Object Asset { get; }
        public string AssetPath { get; }
        public RPGValidationMessage Message { get; }
    }

    public sealed class RPGMapProjectValidationReport
    {
        private readonly List<RPGMapProjectValidationEntry> _entries = new List<RPGMapProjectValidationEntry>();
        public IReadOnlyList<RPGMapProjectValidationEntry> Entries => _entries;
        public int ErrorCount => _entries.Count(entry => entry.Message.IsError);
        public int WarningCount => _entries.Count(entry => entry.Message.Severity == RPGValidationSeverity.Warning);
        public bool Passed => ErrorCount == 0;
        public void Add(RPGMapValidationAssetKind kind, UnityEngine.Object asset, string path, RPGValidationMessage message) => _entries.Add(new RPGMapProjectValidationEntry(kind, asset, path, message));
    }

    public static class RPGMapProjectValidationService
    {
        public static RPGMapProjectValidationReport ValidateAllMapContent()
        {
            var report = new RPGMapProjectValidationReport();
            ValidateSpriteSheets(report);
            ValidateTilesets(report);
            ValidateMaps(report);
            ValidateMapGraphConnections(report);
            ValidateDuplicateAssetIds(report);
            return report;
        }

        public static RPGMapProjectValidationReport ValidateSpriteSheets() { var r = new RPGMapProjectValidationReport(); ValidateSpriteSheets(r); return r; }
        public static RPGMapProjectValidationReport ValidateTilesets() { var r = new RPGMapProjectValidationReport(); ValidateTilesets(r); return r; }
        public static RPGMapProjectValidationReport ValidateMaps() { var r = new RPGMapProjectValidationReport(); ValidateMaps(r); return r; }
        public static RPGMapProjectValidationReport ValidateMapGraphConnections() { var r = new RPGMapProjectValidationReport(); ValidateMapGraphConnections(r); return r; }
        public static RPGMapProjectValidationReport ValidateDuplicateAssetIds() { var r = new RPGMapProjectValidationReport(); ValidateDuplicateAssetIds(r); return r; }

        public static int RepairSafeMapContent()
        {
            var changed = 0;
            foreach (var sheet in FindAssets<RPGSpriteSheetAsset>())
            {
                var frames = sheet.Asset.Frames.Where(frame => frame != null).ToList();
                var touched = false;
                for (var i = 0; i < frames.Count; i++) if (string.IsNullOrWhiteSpace(frames[i].frameId)) { frames[i].frameId = $"{sheet.Asset.name}_frame_{i:000}"; touched = true; }
                if (touched || frames.Count != sheet.Asset.Frames.Count) { sheet.Asset.SetFrames(frames); EditorUtility.SetDirty(sheet.Asset); changed++; }
            }
            foreach (var tileset in FindAssets<RPGTilesetDefinition>())
            {
                var tiles = tileset.Asset.Tiles.Where(tile => tile != null).ToList();
                var touched = false;
                for (var i = 0; i < tiles.Count; i++) if (string.IsNullOrWhiteSpace(tiles[i].tileId)) { tiles[i].tileId = $"{tileset.Asset.name}_tile_{i:000}"; touched = true; }
                var tileIds = new HashSet<string>(tiles.Select(tile => tile.tileId).Where(id => !string.IsNullOrWhiteSpace(id)), StringComparer.OrdinalIgnoreCase);
                var palettes = tileset.Asset.Palettes.Where(palette => palette != null).ToList();
                foreach (var palette in palettes)
                {
                    var rebuilt = (palette.orderedTileIds ?? new List<string>()).Where(tileIds.Contains).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    if (rebuilt.Count != (palette.orderedTileIds?.Count ?? 0)) { palette.orderedTileIds = rebuilt; touched = true; }
                }
                if (touched || tiles.Count != tileset.Asset.Tiles.Count || palettes.Count != tileset.Asset.Palettes.Count) { tileset.Asset.SetTiles(tiles); tileset.Asset.SetPalettes(palettes); EditorUtility.SetDirty(tileset.Asset); changed++; }
            }
            if (changed > 0) AssetDatabase.SaveAssets();
            return changed;
        }

        private static void ValidateSpriteSheets(RPGMapProjectValidationReport report) { foreach (var a in FindAssets<RPGSpriteSheetAsset>()) AddResult(report, RPGMapValidationAssetKind.SpriteSheet, a.Asset, a.Path, a.Asset.ValidateSpriteSheet()); }
        private static void ValidateTilesets(RPGMapProjectValidationReport report) { foreach (var a in FindAssets<RPGTilesetDefinition>()) { AddResult(report, RPGMapValidationAssetKind.Tileset, a.Asset, a.Path, a.Asset.ValidateTileset()); if (a.Asset.SpriteSheet == null) continue; foreach (var tile in a.Asset.Tiles.Where(t => t != null && !string.IsNullOrWhiteSpace(t.sourceFrameId) && a.Asset.SpriteSheet.FindFrame(t.sourceFrameId) == null)) report.Add(RPGMapValidationAssetKind.Tileset, a.Asset, a.Path, new RPGValidationMessage(RPGValidationSeverity.Error, "RPG_MAPCHAIN_TILE_FRAME_REFERENCE", $"{a.Asset.name}/{tile.tileId} references missing sprite frame {tile.sourceFrameId}.", a.Asset.Id)); } }
        private static void ValidateMaps(RPGMapProjectValidationReport report) { foreach (var a in FindAssets<RPGMapDefinition>()) AddResult(report, RPGMapValidationAssetKind.Map, a.Asset, a.Path, a.Asset.ValidateMap()); }
        private static void ValidateMapGraphConnections(RPGMapProjectValidationReport report) { foreach (var a in FindAssets<RPGMapDefinition>()) foreach (var exit in a.Asset.Exits.Where(e => e != null && e.targetMap != null && !string.IsNullOrWhiteSpace(e.targetEntranceId) && e.targetMap.ResolveEntrance(e.targetEntranceId) == null)) report.Add(RPGMapValidationAssetKind.MapGraph, a.Asset, a.Path, new RPGValidationMessage(RPGValidationSeverity.Error, "RPG_MAPGRAPH_MISSING_ENTRANCE", $"{a.Asset.name}/{exit.exitId} targets {exit.targetMap.name} entrance {exit.targetEntranceId}, but it does not exist.", a.Asset.Id)); }
        private static void ValidateDuplicateAssetIds(RPGMapProjectValidationReport report)
        {
            var assets = FindAssets<RPGSpriteSheetAsset>().Select(a => ((RPGObject)a.Asset, a.Path))
                .Concat(FindAssets<RPGTilesetDefinition>().Select(a => ((RPGObject)a.Asset, a.Path)))
                .Concat(FindAssets<RPGMapDefinition>().Select(a => ((RPGObject)a.Asset, a.Path)))
                .Where(a => !a.Asset.Id.IsEmpty)
                .ToList();
            foreach (var group in assets.GroupBy(a => a.Asset.Id, a => a).Where(g => g.Count() > 1))
                foreach (var a in group) report.Add(RPGMapValidationAssetKind.DuplicateId, a.Asset, a.Path, new RPGValidationMessage(RPGValidationSeverity.Error, "RPG_MAP_ASSET_DUPLICATE_ID", $"Duplicate map workflow asset id {group.Key.Value} on {a.Asset.name}.", a.Asset.Id));
        }
        private static void AddResult(RPGMapProjectValidationReport report, RPGMapValidationAssetKind kind, UnityEngine.Object asset, string path, RPGValidationResult result) { foreach (var m in result.Messages) report.Add(kind, asset, path, m); }
        private static IEnumerable<(T Asset, string Path)> FindAssets<T>() where T : UnityEngine.Object { foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}")) { var path = AssetDatabase.GUIDToAssetPath(guid); var asset = AssetDatabase.LoadAssetAtPath<T>(path); if (asset != null) yield return (asset, path); } }
    }
}
