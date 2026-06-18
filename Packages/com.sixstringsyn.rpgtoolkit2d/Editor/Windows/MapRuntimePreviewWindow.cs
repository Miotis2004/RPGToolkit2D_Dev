using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class MapRuntimePreviewWindow : EditorWindow
    {
        private RPGMapDefinition _map;
        private RPGMapCollisionMode _collisionMode = RPGMapCollisionMode.TilemapCollider2D;
        private RPGMapLoader _loader;

        [MenuItem("Tools/RPG Toolkit/Maps/Runtime Map Preview")]
        public static void Open() => GetWindow<MapRuntimePreviewWindow>("RPG Map Preview");

        [MenuItem("Tools/RPG Toolkit/Maps/Load Selected Map In Scene")]
        public static void LoadSelectedMapInScene()
        {
            var map = Selection.activeObject as RPGMapDefinition;
            if (map == null)
            {
                EditorUtility.DisplayDialog("RPG Map Preview", "Select an RPGMapDefinition asset first.", "OK");
                return;
            }

            var loader = Object.FindFirstObjectByType<RPGMapLoader>() ?? new GameObject("RPG Map Loader").AddComponent<RPGMapLoader>();
            loader.LoadMap(map);
            Selection.activeGameObject = loader.LoadedMap?.Root;
            if (loader.LoadedMap?.Root != null) EditorGUIUtility.PingObject(loader.LoadedMap.Root);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Loads an authored RPGMapDefinition into a Unity Grid with Tilemap layers. Requires Unity's built-in Tilemap and Physics 2D modules for tile rendering/colliders.", MessageType.Info);
            _map = (RPGMapDefinition)EditorGUILayout.ObjectField("Map", _map, typeof(RPGMapDefinition), false);
            _collisionMode = (RPGMapCollisionMode)EditorGUILayout.EnumPopup("Collision Mode", _collisionMode);

            using (new EditorGUI.DisabledScope(_map == null))
            {
                if (GUILayout.Button("Load Preview In Current Scene")) LoadPreview();
            }

            using (new EditorGUI.DisabledScope(_loader == null || _loader.LoadedMap == null))
            {
                if (GUILayout.Button("Ping Loaded Map Root"))
                {
                    Selection.activeGameObject = _loader.LoadedMap.Root;
                    EditorGUIUtility.PingObject(_loader.LoadedMap.Root);
                }

                if (GUILayout.Button("Unload Preview")) _loader.UnloadCurrentMap();
            }
        }

        private void LoadPreview()
        {
            _loader = Object.FindFirstObjectByType<RPGMapLoader>() ?? new GameObject("RPG Map Loader").AddComponent<RPGMapLoader>();
            _loader.CollisionMode = _collisionMode;
            _loader.LoadMap(_map);
            Selection.activeGameObject = _loader.LoadedMap.Root;
            EditorGUIUtility.PingObject(_loader.LoadedMap.Root);
        }
    }
}
