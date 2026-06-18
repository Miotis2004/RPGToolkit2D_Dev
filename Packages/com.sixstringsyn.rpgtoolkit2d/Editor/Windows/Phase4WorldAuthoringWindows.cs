using SixStringSyn.RPGToolkit2D.Runtime.Cutscenes;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using SixStringSyn.RPGToolkit2D.Runtime.Schedules;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class MapEditorWindow : AssetPickerWindow
    {
        [MenuItem("Tools/RPG Toolkit/Maps/Map Editor")]
        public static void Open() { var window = GetWindow<MapEditorWindow>("Maps"); window.TypeName = "Map"; }
        protected override void CreateAsset() => ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<RPGMapDefinition>(), "NewRPGMap.asset");
    }

    public sealed class TilesetEditorWindow : AssetPickerWindow
    {
        [MenuItem("Tools/RPG Toolkit/Maps/Tileset Editor")]
        public static void Open() { var window = GetWindow<TilesetEditorWindow>("Tilesets"); window.TypeName = "Tileset"; }
        protected override void CreateAsset() => ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<RPGTilesetDefinition>(), "NewRPGTileset.asset");
    }

    public sealed class NPCScheduleEditorWindow : AssetPickerWindow
    {
        [MenuItem("Tools/RPG Toolkit/World/NPC Schedule Editor")]
        public static void Open() { var window = GetWindow<NPCScheduleEditorWindow>("NPC Schedules"); window.TypeName = "NPC Schedule"; }
        protected override void CreateAsset() => ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<NPCScheduleDefinition>(), "NewNPCSchedule.asset");
    }

    public sealed class CutsceneEditorWindow : AssetPickerWindow
    {
        [MenuItem("Tools/RPG Toolkit/Cutscene Editor")]
        public static void Open() { var window = GetWindow<CutsceneEditorWindow>("Cutscenes"); window.TypeName = "Cutscene"; }
        protected override void CreateAsset() => ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<RPGCutsceneDefinition>(), "NewRPGCutscene.asset");
    }
}
