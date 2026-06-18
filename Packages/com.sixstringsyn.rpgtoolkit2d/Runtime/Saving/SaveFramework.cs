using System;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using SixStringSyn.RPGToolkit2D.Runtime.World;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    [Serializable]
    public sealed class SaveProfile
    {
        public string profileId = "default";
        public string displayName = "Default Profile";
        public string activeSlotId = SaveSlotIds.AutoSave;
        public bool autosaveEnabled = true;
    }

    public static class SaveSlotIds
    {
        public const string AutoSave = "autosave";
        public static string Manual(int index) => "manual_" + Math.Max(1, index);
    }

    public interface ICloudSaveProvider
    {
        SaveResult Upload(string slotId, string json);
        SaveResult Download(string slotId, out string json);
    }

    public sealed class DelegateSaveMigration : ISaveMigration
    {
        private readonly string _fromVersion;
        private readonly string _toVersion;
        private readonly Func<GameSaveData, GameSaveData> _migrate;
        public DelegateSaveMigration(string fromVersion, string toVersion, Func<GameSaveData, GameSaveData> migrate)
        { _fromVersion = fromVersion; _toVersion = toVersion; _migrate = migrate; }
        public bool CanMigrate(string fromVersion, string toVersion) => string.Equals(fromVersion, _fromVersion, StringComparison.OrdinalIgnoreCase) && string.Equals(toVersion, _toVersion, StringComparison.OrdinalIgnoreCase);
        public GameSaveData Migrate(GameSaveData saveData, string toVersion) { var migrated = _migrate != null ? _migrate(saveData) : saveData; if (migrated?.metadata != null) migrated.metadata.saveVersion = toVersion; return migrated; }
    }

    public sealed class WorldStateSaveContributor : ISaveContributor
    {
        private readonly WorldState _state;
        public WorldStateSaveContributor(WorldState state, string systemId = "world_state") { _state = state; SystemId = systemId; }
        public string SystemId { get; }
        public string CaptureJson() => JsonUtility.ToJson(_state?.Capture() ?? new Runtime.World.WorldStateSaveData());
        public void RestoreJson(string json) => _state?.Restore(JsonUtility.FromJson<Runtime.World.WorldStateSaveData>(json));
    }

    public sealed class QuestSaveContributor : ISaveContributor
    {
        private readonly QuestTracker _tracker;
        private readonly System.Collections.Generic.IEnumerable<QuestDefinition> _definitions;
        public QuestSaveContributor(QuestTracker tracker, System.Collections.Generic.IEnumerable<QuestDefinition> definitions, string systemId = "quests")
        { _tracker = tracker; _definitions = definitions; SystemId = systemId; }
        public string SystemId { get; }
        public string CaptureJson() => JsonUtility.ToJson(_tracker?.ToSaveData() ?? new QuestJournalSaveData());
        public void RestoreJson(string json) => _tracker?.Load(JsonUtility.FromJson<QuestJournalSaveData>(json), _definitions);
    }

    [Serializable]
    public sealed class PlayerPositionSaveData { public string sceneName; public Vector3 position; public Vector3 eulerAngles; }

    public sealed class PlayerPositionSaveContributor : ISaveContributor
    {
        private readonly Func<Transform> _transformGetter;
        private readonly string _sceneName;
        public PlayerPositionSaveContributor(Func<Transform> transformGetter, string sceneName = null, string systemId = "player_position")
        { _transformGetter = transformGetter; _sceneName = sceneName; SystemId = systemId; }
        public string SystemId { get; }
        public string CaptureJson()
        {
            var t = _transformGetter?.Invoke();
            return JsonUtility.ToJson(new PlayerPositionSaveData { sceneName = _sceneName, position = t != null ? t.position : Vector3.zero, eulerAngles = t != null ? t.eulerAngles : Vector3.zero });
        }
        public void RestoreJson(string json)
        {
            var t = _transformGetter?.Invoke();
            if (t == null) return;
            var data = JsonUtility.FromJson<PlayerPositionSaveData>(json);
            t.position = data.position;
            t.eulerAngles = data.eulerAngles;
        }
    }
}
