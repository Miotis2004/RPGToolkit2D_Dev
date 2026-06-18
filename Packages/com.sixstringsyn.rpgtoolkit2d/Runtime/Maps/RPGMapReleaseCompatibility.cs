using System;
using System.Collections.Generic;
using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Core;

namespace SixStringSyn.RPGToolkit2D.Runtime.Maps
{
    /// <summary>
    /// Release hardening helpers for the maps, tilesets, and spritesheets workflow.
    /// Provides a stable catalog of validation diagnostic codes and compatibility checks
    /// that are safe to run in runtime assemblies without editor-only dependencies.
    /// </summary>
    public static class RPGMapReleaseCompatibility
    {
        /// <summary>The released schema version for first-class map workflow assets.</summary>
        public const string ReleasedMapWorkflowSchemaVersion = "1.0.0";

        /// <summary>Stable validation diagnostic codes emitted by map workflow validators.</summary>
        public static readonly IReadOnlyList<string> StableDiagnosticCodes = new[]
        {
            "RPG_SHEET_MISSING_TEXTURE", "RPG_SHEET_MISSING_PROFILE", "RPG_SHEET_INVALID_CELL_SIZE", "RPG_SHEET_FRAME_OUT_OF_BOUNDS", "RPG_SHEET_DUPLICATE_FRAME_ID",
            "RPG_SHEET_DUPLICATE_GRID_POSITION", "RPG_SHEET_MISSING_SPRITE_REFERENCE", "RPG_SHEET_PPU_MISMATCH", "RPG_SHEET_INVALID_FRAME_ID",
            "RPG_TILESET_MISSING_SPRITESHEET", "RPG_TILESET_UNKNOWN_FRAME_ID", "RPG_TILESET_MISSING_SPRITE", "RPG_TILESET_DUPLICATE_TILE_ID", "RPG_TILESET_INVALID_PALETTE_REFERENCE",
            "RPG_TILESET_COLLISION_LAYER_CONFLICT", "RPG_TILESET_ANIMATION_MISSING_FRAMES", "RPG_TILESET_AUTOTILE_INCOMPLETE",
            "RPG_MAP_MISSING_TILESET", "RPG_MAP_INVALID_SIZE", "RPG_MAP_NULL_LAYER", "RPG_MAP_EMPTY_LAYER_ID", "RPG_MAP_DUPLICATE_LAYER_ID", "RPG_MAP_DUPLICATE_LAYER_ORDER",
            "RPG_MAP_INVALID_LAYER_OPACITY", "RPG_MAP_TILE_OUT_OF_BOUNDS", "RPG_MAP_UNKNOWN_TILE", "RPG_MAP_OVERLAPPING_BLOCKER", "RPG_MAP_INVALID_TILE_METADATA_KEY",
            "RPG_MAP_NULL_OBJECT", "RPG_MAP_EMPTY_OBJECT_ID", "RPG_MAP_DUPLICATE_OBJECT_ID", "RPG_MAP_OBJECT_OUT_OF_BOUNDS", "RPG_MAP_INVALID_OBJECT_METADATA_KEY",
            "RPG_MAP_EMPTY_ENTRANCE_ID", "RPG_MAP_DUPLICATE_ENTRANCE_ID", "RPG_MAP_ENTRANCE_OUT_OF_BOUNDS", "RPG_MAP_EMPTY_EXIT_ID", "RPG_MAP_DUPLICATE_EXIT_ID",
            "RPG_MAP_EXIT_OUT_OF_BOUNDS", "RPG_MAP_EXIT_MISSING_TARGET", "RPG_MAP_EXIT_MISSING_TARGET_ENTRANCE", "RPG_MAP_BROKEN_CONNECTION", "RPG_MAP_EMPTY_ZONE_ID",
            "RPG_MAP_DUPLICATE_ZONE_ID", "RPG_MAP_ZONE_OUT_OF_BOUNDS"
        };

        /// <summary>
        /// Validates that all messages use release-stable diagnostic codes and contain actionable text.
        /// </summary>
        public static RPGValidationResult ValidateDiagnosticsForRelease(IEnumerable<RPGValidationMessage> messages)
        {
            var result = new RPGValidationResult();
            var stableCodes = new HashSet<string>(StableDiagnosticCodes, StringComparer.OrdinalIgnoreCase);
            foreach (var message in messages ?? Enumerable.Empty<RPGValidationMessage>())
            {
                if (string.IsNullOrWhiteSpace(message.Code) || !stableCodes.Contains(message.Code))
                    result.AddError("RPG_RELEASE_UNSTABLE_DIAGNOSTIC_CODE", $"Diagnostic '{message.Code}' is not registered as a stable release code.");
                if (string.IsNullOrWhiteSpace(message.Message) || message.Message.Length < 12)
                    result.AddError("RPG_RELEASE_UNACTIONABLE_DIAGNOSTIC", $"Diagnostic '{message.Code}' needs an actionable designer-facing message.");
            }

            return result;
        }

        /// <summary>
        /// Runs map migration and validation, then verifies diagnostics meet release requirements.
        /// </summary>
        public static RPGValidationResult ValidateMapForRelease(RPGMapDefinition map)
        {
            var result = new RPGValidationResult();
            if (map == null)
            {
                result.AddError("RPG_RELEASE_NULL_MAP", "No map was supplied for release compatibility validation.");
                return result;
            }

            map.MigrateExpandedMapData();
            var mapResult = map.ValidateMap();
            foreach (var message in mapResult.Messages)
                result.Add(message.Severity, message.Code, message.Message, message.RelatedId);
            foreach (var message in ValidateDiagnosticsForRelease(mapResult.Messages).Messages)
                result.Add(message.Severity, message.Code, message.Message, message.RelatedId);
            return result;
        }
    }
}
