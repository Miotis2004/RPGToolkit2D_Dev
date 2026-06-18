using System.Linq;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.PhaseMaps
{
    public sealed class MapReleaseCompatibilityTests
    {
        [Test]
        public void StableDiagnosticCatalogIncludesExpandedMapCodes()
        {
            CollectionAssert.Contains(RPGMapReleaseCompatibility.StableDiagnosticCodes.ToArray(), "RPG_MAP_DUPLICATE_LAYER_ID");
            CollectionAssert.Contains(RPGMapReleaseCompatibility.StableDiagnosticCodes.ToArray(), "RPG_MAP_EXIT_MISSING_TARGET_ENTRANCE");
            CollectionAssert.Contains(RPGMapReleaseCompatibility.StableDiagnosticCodes.ToArray(), "RPG_TILESET_DUPLICATE_TILE_ID");
            Assert.AreEqual("1.0.0", RPGMapReleaseCompatibility.ReleasedMapWorkflowSchemaVersion);
        }

        [Test]
        public void ReleaseDiagnosticValidationRejectsUnregisteredCodesAndEmptyMessages()
        {
            var messages = new[]
            {
                new RPGValidationMessage(RPGValidationSeverity.Error, "RPG_MAP_INVALID_SIZE", "Map must have a positive size before release."),
                new RPGValidationMessage(RPGValidationSeverity.Warning, "RPG_MAP_EXPERIMENTAL_CODE", "This code was not registered for release."),
                new RPGValidationMessage(RPGValidationSeverity.Warning, "RPG_MAP_INVALID_SIZE", "Short")
            };

            var codes = RPGMapReleaseCompatibility.ValidateDiagnosticsForRelease(messages).Messages.Select(message => message.Code).ToArray();

            CollectionAssert.Contains(codes, "RPG_RELEASE_UNSTABLE_DIAGNOSTIC_CODE");
            CollectionAssert.Contains(codes, "RPG_RELEASE_UNACTIONABLE_DIAGNOSTIC");
        }

        [Test]
        public void ReleaseMapValidationMigratesLegacyDataBeforeCheckingDiagnostics()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            var layers = (System.Collections.IList)typeof(RPGMapDefinition).GetField("_layers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(map);
            layers.Add(new RPGMapLayer { layerId = string.Empty, displayName = string.Empty, tiles = { new RPGMapTile { tileId = "missing", position = new Vector2Int(99, 99) } } });

            var result = RPGMapReleaseCompatibility.ValidateMapForRelease(map);

            Assert.AreEqual("Layer_1", map.Layers[0].layerId);
            CollectionAssert.Contains(result.Messages.Select(message => message.Code).ToArray(), "RPG_MAP_TILE_OUT_OF_BOUNDS");
            CollectionAssert.DoesNotContain(result.Messages.Select(message => message.Code).ToArray(), "RPG_RELEASE_UNSTABLE_DIAGNOSTIC_CODE");
        }
    }
}
