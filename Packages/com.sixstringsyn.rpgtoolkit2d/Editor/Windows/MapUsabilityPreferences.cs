using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEditor;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public static class MapUsabilityPreferences
    {
        private const int MaxRecentItems = 12;
        private const string RecentMapsKey = "RPGToolkit2D.Maps.RecentMaps";
        private const string RecentTilesetsKey = "RPGToolkit2D.Maps.RecentTilesets";
        private const string FavoriteMapsKey = "RPGToolkit2D.Maps.FavoriteMaps";
        private const string FavoriteTilesetsKey = "RPGToolkit2D.Maps.FavoriteTilesets";
        private const string PaletteSearchKey = "RPGToolkit2D.Maps.PaletteSearch";

        public static IReadOnlyList<string> RecentMaps => ReadList(RecentMapsKey);
        public static IReadOnlyList<string> RecentTilesets => ReadList(RecentTilesetsKey);
        public static IReadOnlyList<string> FavoriteMaps => ReadList(FavoriteMapsKey);
        public static IReadOnlyList<string> FavoriteTilesets => ReadList(FavoriteTilesetsKey);
        public static string PaletteSearch { get => EditorPrefs.GetString(PaletteSearchKey, string.Empty); set => EditorPrefs.SetString(PaletteSearchKey, value ?? string.Empty); }

        public static void RememberMap(RPGMapDefinition map) => RememberAsset(RecentMapsKey, map);
        public static void RememberTileset(RPGTilesetDefinition tileset) => RememberAsset(RecentTilesetsKey, tileset);
        public static void ToggleFavoriteMap(RPGMapDefinition map) => ToggleFavoriteAsset(FavoriteMapsKey, map);
        public static void ToggleFavoriteTileset(RPGTilesetDefinition tileset) => ToggleFavoriteAsset(FavoriteTilesetsKey, tileset);

        public static IEnumerable<RPGTileDefinition> SearchPalette(RPGTilesetDefinition tileset, string query)
        {
            if (tileset == null) return Enumerable.Empty<RPGTileDefinition>();
            if (string.IsNullOrWhiteSpace(query)) return tileset.Tiles.Where(tile => tile != null);
            return tileset.Tiles.Where(tile => tile != null && Matches(tile, query));
        }

        private static bool Matches(RPGTileDefinition tile, string query)
        {
            return Contains(tile.tileId, query)
                || Contains(tile.sourceFrameId, query)
                || Contains(tile.category.ToString(), query)
                || tile.AllTerrainTags().Any(tag => Contains(tag, query))
                || (tile.customMetadata ?? Enumerable.Empty<RPGTileCustomMetadata>()).Any(entry => Contains(entry?.key, query) || Contains(entry?.value, query));
        }

        private static bool Contains(string value, string query) => !string.IsNullOrWhiteSpace(value) && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

        private static void RememberAsset(string key, UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrWhiteSpace(path)) return;
            var values = ReadList(key).Where(existing => !string.Equals(existing, path, StringComparison.OrdinalIgnoreCase)).Prepend(path).Take(MaxRecentItems);
            WriteList(key, values);
        }

        private static void ToggleFavoriteAsset(string key, UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrWhiteSpace(path)) return;
            var values = ReadList(key).ToList();
            values.RemoveAll(existing => string.Equals(existing, path, StringComparison.OrdinalIgnoreCase));
            if (!ReadList(key).Any(existing => string.Equals(existing, path, StringComparison.OrdinalIgnoreCase))) values.Add(path);
            WriteList(key, values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
        }

        private static IReadOnlyList<string> ReadList(string key) => EditorPrefs.GetString(key, string.Empty).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        private static void WriteList(string key, IEnumerable<string> values) => EditorPrefs.SetString(key, string.Join("|", values ?? Enumerable.Empty<string>()));
    }
}
