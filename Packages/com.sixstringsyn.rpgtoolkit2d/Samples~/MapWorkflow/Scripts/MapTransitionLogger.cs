using UnityEngine;
using SixStringSyn.RPGToolkit2D.Maps;

namespace SixStringSyn.RPGToolkit2D.Samples.MapWorkflow
{
    public sealed class MapTransitionLogger : MonoBehaviour, IRPGMapTransitionHandler
    {
        public void HandleTransition(RPGMapDefinition sourceMap, RPGMapExit exit, RPGMapDefinition targetMap, RPGMapEntrance targetEntrance)
        {
            var sourceName = sourceMap != null ? sourceMap.name : "<missing source>";
            var exitId = exit != null ? exit.exitId : "<missing exit>";
            var targetName = targetMap != null ? targetMap.name : "<missing target>";
            var entranceId = targetEntrance != null ? targetEntrance.entranceId : "<missing entrance>";
            Debug.Log($"Map transition resolved: {sourceName}/{exitId} -> {targetName}/{entranceId}");
        }
    }
}
