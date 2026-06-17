using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    public sealed class SaveGameService
    {
        private readonly List<ISaveContributor> _contributors = new List<ISaveContributor>();
        private readonly List<ISaveMigration> _migrations = new List<ISaveMigration>();

        public IReadOnlyList<ISaveContributor> Contributors => _contributors;

        public void Register(ISaveContributor contributor)
        {
            if (contributor != null && !_contributors.Contains(contributor)) _contributors.Add(contributor);
        }

        public void Unregister(ISaveContributor contributor) => _contributors.Remove(contributor);
        public void RegisterMigration(ISaveMigration migration) { if (migration != null) _migrations.Add(migration); }

        public GameSaveData Capture(double playtimeSeconds = 0d, string sceneName = null)
        {
            var now = DateTime.UtcNow.ToString("o");
            var data = new GameSaveData();
            data.metadata.createdUtc = now;
            data.metadata.updatedUtc = now;
            data.metadata.playtimeSeconds = Math.Max(0d, playtimeSeconds);
            data.metadata.sceneName = sceneName ?? SceneManager.GetActiveScene().name;
            foreach (var contributor in _contributors)
            {
                data.SetSystemJson(contributor.SystemId, contributor.CaptureJson());
            }
            return data;
        }

        public SaveResult Restore(GameSaveData data)
        {
            if (data == null) return SaveResult.Fail("Save data is missing.");
            if (data.metadata != null && data.metadata.saveVersion != SaveConstants.CurrentSaveVersion)
            {
                data = TryMigrate(data);
                if (data == null) return SaveResult.Fail("Save version is not supported and no migration was available.");
            }
            foreach (var contributor in _contributors)
            {
                if (data.TryGetSystemJson(contributor.SystemId, out var json)) contributor.RestoreJson(json);
            }
            return SaveResult.Ok();
        }

        public string ToJson(GameSaveData data, bool prettyPrint = true) => JsonUtility.ToJson(data, prettyPrint);
        public GameSaveData FromJson(string json) => string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<GameSaveData>(json);

        private GameSaveData TryMigrate(GameSaveData data)
        {
            foreach (var migration in _migrations)
            {
                if (migration.CanMigrate(data.metadata.saveVersion, SaveConstants.CurrentSaveVersion)) return migration.Migrate(data, SaveConstants.CurrentSaveVersion);
            }
            return null;
        }
    }
}
