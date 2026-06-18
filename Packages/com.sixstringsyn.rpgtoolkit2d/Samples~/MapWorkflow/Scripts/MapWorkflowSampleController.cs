using UnityEngine;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;

namespace SixStringSyn.RPGToolkit2D.Samples.MapWorkflow
{
    public sealed class MapWorkflowSampleController : MonoBehaviour
    {
        [SerializeField] private RPGMapLoader loader;
        [SerializeField] private RPGMapDefinition startingMap;
        [SerializeField] private string entranceId;
        [SerializeField] private Vector2Int queryCell = Vector2Int.zero;
        [SerializeField] private RectInt objectQueryRegion = new RectInt(0, 0, 8, 8);
        [SerializeField] private string transitionExitId;
        [SerializeField] private MapTransitionLogger transitionLogger;

        private void Awake()
        {
            if (loader == null) loader = GetComponent<RPGMapLoader>();
            if (transitionLogger == null) transitionLogger = GetComponent<MapTransitionLogger>();
            if (loader != null && transitionLogger != null) loader.TransitionHandler = transitionLogger;
        }

        private void Start()
        {
            if (loader == null || startingMap == null)
            {
                Debug.LogWarning("Assign an RPGMapLoader and starting RPGMapDefinition before running the Map Workflow sample.");
                return;
            }

            loader.LoadMap(startingMap, entranceId);
            LogQueries(startingMap);

            if (!string.IsNullOrWhiteSpace(transitionExitId) && !loader.TryTransition(transitionExitId))
            {
                Debug.LogWarning($"Transition '{transitionExitId}' could not be resolved. Check target map and entrance IDs.");
            }
        }

        private void LogQueries(RPGMapDefinition map)
        {
            Debug.Log($"Cell {queryCell} blocked: {map.IsBlocked(queryCell)}");

            var zone = map.GetHighestPriorityZoneAt(queryCell);
            Debug.Log(zone == null
                ? $"No zone at {queryCell}."
                : $"Highest-priority zone at {queryCell}: {zone.zoneId} ({zone.kind}) payload={zone.payloadId}");

            foreach (var placement in map.GetObjectsInRegion(objectQueryRegion))
            {
                Debug.Log($"Object in sample region: {placement.objectId} at {placement.gridPosition} category={placement.category}");
            }
        }
    }
}
