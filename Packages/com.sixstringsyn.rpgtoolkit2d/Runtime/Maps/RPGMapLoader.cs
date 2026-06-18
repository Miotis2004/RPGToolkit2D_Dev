using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SixStringSyn.RPGToolkit2D.Runtime.Maps
{
    public enum RPGMapCollisionMode { None, TilemapCollider2D, GeneratedColliderObjects, QueryOnly }

    public interface IRPGMapTileResolver
    {
        TileBase ResolveTile(RPGTileDefinition tileDefinition, RPGMapTile placement);
    }

    public interface IRPGMapSpawnInitializer
    {
        void Initialize(RPGMapDefinition map, RPGMapObjectSpawnDescriptor descriptor);
    }

    public sealed class RPGGeneratedTileResolver : IRPGMapTileResolver
    {
        private readonly Dictionary<string, Tile> _runtimeTiles = new Dictionary<string, Tile>(StringComparer.OrdinalIgnoreCase);

        public TileBase ResolveTile(RPGTileDefinition tileDefinition, RPGMapTile placement)
        {
            if (tileDefinition == null) return null;
            var key = string.IsNullOrWhiteSpace(tileDefinition.tileId) ? tileDefinition.GetHashCode().ToString() : tileDefinition.tileId;
            if (_runtimeTiles.TryGetValue(key, out var cached)) return cached;
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = $"RPG Runtime Tile - {key}";
            tile.sprite = tileDefinition.sprite;
            tile.colliderType = tileDefinition.blocksMovement || tileDefinition.collisionShapeMode != RPGTileCollisionShapeMode.None ? Tile.ColliderType.Sprite : Tile.ColliderType.None;
            _runtimeTiles[key] = tile;
            return tile;
        }
    }

    public sealed class RPGLoadedMap
    {
        public RPGMapDefinition Map { get; internal set; }
        public GameObject Root { get; internal set; }
        public Grid Grid { get; internal set; }
        public IReadOnlyDictionary<string, Tilemap> TilemapsByLayerId => _tilemapsByLayerId;
        public IReadOnlyList<GameObject> SpawnedObjects => _spawnedObjects;
        public IReadOnlyList<GameObject> GeneratedColliders => _generatedColliders;

        internal readonly Dictionary<string, Tilemap> _tilemapsByLayerId = new Dictionary<string, Tilemap>(StringComparer.OrdinalIgnoreCase);
        internal readonly List<GameObject> _spawnedObjects = new List<GameObject>();
        internal readonly List<GameObject> _generatedColliders = new List<GameObject>();
    }

    public sealed class RPGMapLoader : MonoBehaviour, IRPGMapPrefabResolver, IRPGMapObjectStateResolver
    {
        [SerializeField] private RPGMapDefinition _initialMap;
        [SerializeField] private Transform _player;
        [SerializeField] private RPGMapCollisionMode _collisionMode = RPGMapCollisionMode.TilemapCollider2D;
        [SerializeField] private bool _loadOnStart;
        [SerializeField] private bool _unloadPreviousMap = true;
        [SerializeField] private Vector3 _cellSize = Vector3.one;

        private readonly IRPGMapTileResolver _generatedTileResolver = new RPGGeneratedTileResolver();
        private RPGLoadedMap _loadedMap;

        public event Action<RPGLoadedMap> MapLoaded;
        public event Action<RPGLoadedMap> MapUnloaded;
        public event Action<RPGMapDefinition, RPGMapExit, RPGMapDefinition, RPGMapEntrance> TransitionRequested;
        public event Action<GameObject, RPGMapObjectSpawnDescriptor> ObjectSpawned;

        public RPGLoadedMap LoadedMap => _loadedMap;
        public RPGMapCollisionMode CollisionMode { get => _collisionMode; set => _collisionMode = value; }
        public IRPGMapTileResolver TileResolver { get; set; }
        public IRPGMapPrefabResolver PrefabResolver { get; set; }
        public IRPGMapObjectStateResolver StateResolver { get; set; }
        public IRPGMapTransitionHandler TransitionHandler { get; set; }

        private void Start()
        {
            if (_loadOnStart && _initialMap != null) LoadMap(_initialMap);
        }

        public RPGLoadedMap LoadMap(RPGMapDefinition map, string entranceId = null)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (_unloadPreviousMap) UnloadCurrentMap();

            var root = new GameObject($"RPG Map - {map.name}");
            root.transform.SetParent(transform, false);
            var grid = root.AddComponent<Grid>();
            grid.cellSize = _cellSize;

            var loaded = new RPGLoadedMap { Map = map, Root = root, Grid = grid };
            BuildTilemaps(map, loaded);
            SpawnObjects(map, loaded);
            PositionPlayerAtEntrance(map, entranceId);
            _loadedMap = loaded;
            MapLoaded?.Invoke(loaded);
            return loaded;
        }

        public void UnloadCurrentMap()
        {
            if (_loadedMap == null) return;
            var unloaded = _loadedMap;
            if (unloaded.Root != null) DestroyImmediateSafe(unloaded.Root);
            _loadedMap = null;
            MapUnloaded?.Invoke(unloaded);
        }

        public bool TryTransition(string exitId)
        {
            if (_loadedMap?.Map == null) return false;
            var resolution = _loadedMap.Map.ResolveExitConnection(exitId);
            if (!resolution.IsResolved) return false;
            TransitionRequested?.Invoke(_loadedMap.Map, resolution.Exit, resolution.TargetMap, resolution.TargetEntrance);
            TransitionHandler?.HandleTransition(_loadedMap.Map, resolution.Exit, resolution.TargetMap, resolution.TargetEntrance);
            LoadMap(resolution.TargetMap, resolution.TargetEntrance?.entranceId);
            return true;
        }

        public GameObject ResolvePrefab(RPGMapObjectPlacement placement) => placement?.prefab;
        public bool ShouldSpawn(RPGMapObjectPlacement placement) => true;

        private void BuildTilemaps(RPGMapDefinition map, RPGLoadedMap loaded)
        {
            foreach (var layer in map.Layers.Where(layer => layer != null).OrderBy(layer => layer.renderOrder))
            {
                var layerObject = new GameObject(string.IsNullOrWhiteSpace(layer.unityTilemapTargetName) ? layer.layerId : layer.unityTilemapTargetName);
                layerObject.transform.SetParent(loaded.Root.transform, false);
                var tilemap = layerObject.AddComponent<Tilemap>();
                var renderer = layerObject.AddComponent<TilemapRenderer>();
                renderer.sortingOrder = layer.renderOrder;
                tilemap.color = new Color(1f, 1f, 1f, Mathf.Clamp01(layer.opacity));
                tilemap.gameObject.SetActive(layer.visible);

                foreach (var placement in layer.tiles ?? Enumerable.Empty<RPGMapTile>())
                {
                    var tileDefinition = map.Tileset?.FindTile(placement.tileId);
                    var tile = (TileResolver ?? _generatedTileResolver).ResolveTile(tileDefinition, placement);
                    if (tile == null) continue;
                    var cell = new Vector3Int(placement.position.x, placement.position.y, 0);
                    tilemap.SetTile(cell, tile);
                    tilemap.SetTransformMatrix(cell, BuildTileTransform(placement));
                }

                if (_collisionMode == RPGMapCollisionMode.TilemapCollider2D && LayerNeedsCollider(map, layer)) layerObject.AddComponent<TilemapCollider2D>();
                loaded._tilemapsByLayerId[layer.layerId] = tilemap;
            }

            if (_collisionMode == RPGMapCollisionMode.GeneratedColliderObjects) BuildGeneratedColliders(map, loaded);
        }

        private void SpawnObjects(RPGMapDefinition map, RPGLoadedMap loaded)
        {
            var container = new GameObject("Objects");
            container.transform.SetParent(loaded.Root.transform, false);
            foreach (var descriptor in RPGMapObjectSpawner.BuildSpawnDescriptors(map, PrefabResolver ?? this, StateResolver ?? this))
            {
                if (descriptor.prefab == null) continue;
                var instance = Instantiate(descriptor.prefab, descriptor.worldPosition, descriptor.rotation, container.transform);
                instance.name = string.IsNullOrWhiteSpace(descriptor.displayName) ? descriptor.objectId : descriptor.displayName;
                instance.transform.localScale = descriptor.scale;
                foreach (var initializer in instance.GetComponentsInChildren<IRPGMapSpawnInitializer>()) initializer.Initialize(map, descriptor);
                loaded._spawnedObjects.Add(instance);
                ObjectSpawned?.Invoke(instance, descriptor);
            }
        }

        private void BuildGeneratedColliders(RPGMapDefinition map, RPGLoadedMap loaded)
        {
            var container = new GameObject("Generated Colliders");
            container.transform.SetParent(loaded.Root.transform, false);
            for (var y = 0; y < map.Size.y; y++)
            for (var x = 0; x < map.Size.x; x++)
            {
                var position = new Vector2Int(x, y);
                if (!map.IsBlocked(position)) continue;
                var colliderObject = new GameObject($"Blocker {x},{y}");
                colliderObject.transform.SetParent(container.transform, false);
                colliderObject.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0f);
                colliderObject.AddComponent<BoxCollider2D>().size = Vector2.one;
                loaded._generatedColliders.Add(colliderObject);
            }
        }

        private void PositionPlayerAtEntrance(RPGMapDefinition map, string entranceId)
        {
            if (_player == null || string.IsNullOrWhiteSpace(entranceId)) return;
            var entrance = map.ResolveEntrance(entranceId);
            if (entrance != null) _player.position = new Vector3(entrance.position.x + 0.5f, entrance.position.y + 0.5f, _player.position.z);
        }

        private static bool LayerNeedsCollider(RPGMapDefinition map, RPGMapLayer layer) => layer.kind == RPGMapLayerKind.Collision || (layer.tiles ?? Enumerable.Empty<RPGMapTile>()).Any(tile => map.Tileset?.FindTile(tile.tileId)?.blocksMovement == true || (tile.overrideCollision && tile.blocksMovementOverride));

        private static Matrix4x4 BuildTileTransform(RPGMapTile tile)
        {
            var rotation = Quaternion.Euler(0f, 0f, tile?.rotationDegrees ?? 0);
            var scale = new Vector3(tile?.flipX == true ? -1f : 1f, tile?.flipY == true ? -1f : 1f, 1f);
            return Matrix4x4.TRS(Vector3.zero, rotation, scale);
        }

        private static void DestroyImmediateSafe(UnityEngine.Object target)
        {
            if (target == null) return;
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }
    }
}
