using System.Linq;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Cutscenes;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using SixStringSyn.RPGToolkit2D.Runtime.Schedules;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Phase4
{
    public sealed class WorldSimulationPresentationTests
    {
        [Test]
        public void ScheduleResolverWrapsToLatestPriorEntry()
        {
            var schedule = ScriptableObject.CreateInstance<NPCScheduleDefinition>();
            var entries = (System.Collections.IList)typeof(NPCScheduleDefinition).GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(schedule);
            entries.Add(new NPCScheduleEntry { time = new RPGTimeOfDay { hour = 8 }, destinationId = "Home" });
            entries.Add(new NPCScheduleEntry { time = new RPGTimeOfDay { hour = 17 }, destinationId = "Tavern" });

            Assert.AreEqual("Home", schedule.Resolve(new RPGTimeOfDay { hour = 9 }).destinationId);
            Assert.AreEqual("Tavern", schedule.Resolve(new RPGTimeOfDay { hour = 2 }).destinationId);
        }

        [Test]
        public void CutsceneValidationFindsBrokenLinks()
        {
            var cutscene = ScriptableObject.CreateInstance<RPGCutsceneDefinition>();
            typeof(RPGCutsceneDefinition).GetField("_startNodeId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(cutscene, "start");
            var nodes = (System.Collections.IList)typeof(RPGCutsceneDefinition).GetField("_nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cutscene);
            nodes.Add(new RPGCutsceneNode { nodeId = "start", kind = RPGCutsceneNodeKind.Dialogue, nextNodeIds = { "missing" } });

            var result = cutscene.ValidateCutscene();

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Messages.Any(message => message.Code == "RPG_CUTSCENE_BROKEN_LINK"));
        }

        [Test]
        public void MapZonesCanBeQueriedByPositionAndKind()
        {
            var map = ScriptableObject.CreateInstance<RPGMapDefinition>();
            var zones = (System.Collections.IList)typeof(RPGMapDefinition).GetField("_zones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(map);
            zones.Add(new RPGMapZone { zoneId = "forest", kind = RPGMapZoneKind.Encounter, bounds = new RectInt(1, 1, 3, 3), payloadId = "slimes" });

            using var enumerator = map.GetZonesAt(new Vector2Int(2, 2), RPGMapZoneKind.Encounter).GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("forest", enumerator.Current.zoneId);
        }
    }
}
