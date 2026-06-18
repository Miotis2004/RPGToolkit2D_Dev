using System;
using System.Collections.Generic;

namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    [Serializable]
    public sealed class SaveMetadata
    {
        public string saveVersion = SaveConstants.CurrentSaveVersion;
        public string packageVersion = "1.0.0";
        public string createdUtc;
        public string updatedUtc;
        public double playtimeSeconds;
        public string sceneName;
    }

    [Serializable]
    public sealed class SystemSaveData
    {
        public string systemId;
        public string json;
    }

    [Serializable]
    public sealed class GameSaveData
    {
        public SaveMetadata metadata = new SaveMetadata();
        public List<SystemSaveData> systems = new List<SystemSaveData>();

        public bool TryGetSystemJson(string systemId, out string json)
        {
            foreach (var system in systems)
            {
                if (system.systemId == systemId)
                {
                    json = system.json;
                    return true;
                }
            }
            json = null;
            return false;
        }

        public void SetSystemJson(string systemId, string json)
        {
            var existing = systems.Find(s => s.systemId == systemId);
            if (existing == null)
            {
                systems.Add(new SystemSaveData { systemId = systemId, json = json ?? string.Empty });
            }
            else
            {
                existing.json = json ?? string.Empty;
            }
        }
    }

    public static class SaveConstants
    {
        public const string CurrentSaveVersion = "1.0.0";
    }
}
