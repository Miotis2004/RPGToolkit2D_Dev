using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    public sealed class SaveSlotInfo
    {
        public string SlotId { get; set; }
        public string Path { get; set; }
        public SaveMetadata Metadata { get; set; }
    }

    public sealed class SaveSlotService
    {
        private readonly string _rootPath;
        private readonly SaveGameService _saveGameService;
        private ICloudSaveProvider _cloudSaveProvider;

        public SaveSlotService(SaveGameService saveGameService, string rootPath = null)
        {
            _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
            _rootPath = string.IsNullOrWhiteSpace(rootPath) ? Path.Combine(Application.persistentDataPath, "RPGToolkit2D", "Saves") : rootPath;
        }

        public string RootPath => _rootPath;
        public string GetSlotPath(string slotId) => Path.Combine(_rootPath, Sanitize(slotId) + ".json");
        public bool Exists(string slotId) => File.Exists(GetSlotPath(slotId));
        public void SetCloudSaveProvider(ICloudSaveProvider provider) => _cloudSaveProvider = provider;
        public SaveResult SaveAutosave(GameSaveData data) => Save(SaveSlotIds.AutoSave, data);
        public SaveResult SaveManual(int slotIndex, GameSaveData data) => Save(SaveSlotIds.Manual(slotIndex), data);

        public SaveResult Save(string slotId, GameSaveData data)
        {
            try
            {
                Directory.CreateDirectory(_rootPath);
                if (data.metadata == null) data.metadata = new SaveMetadata();
                data.metadata.updatedUtc = DateTime.UtcNow.ToString("o");
                File.WriteAllText(GetSlotPath(slotId), _saveGameService.ToJson(data));
                var path = GetSlotPath(slotId);
                if (_cloudSaveProvider != null)
                {
                    var upload = _cloudSaveProvider.Upload(slotId, File.ReadAllText(path));
                    if (!upload.Success) return upload;
                }
                return SaveResult.Ok(path);
            }
            catch (Exception ex) { return SaveResult.Fail(ex.Message); }
        }

        public SaveResult Load(string slotId, out GameSaveData data)
        {
            data = null;
            try
            {
                var path = GetSlotPath(slotId);
                if (!File.Exists(path))
                {
                    if (_cloudSaveProvider == null) return SaveResult.Fail("Save slot does not exist.");
                    var download = _cloudSaveProvider.Download(slotId, out var cloudJson);
                    if (!download.Success) return download;
                    Directory.CreateDirectory(_rootPath);
                    File.WriteAllText(path, cloudJson ?? string.Empty);
                }
                data = _saveGameService.FromJson(File.ReadAllText(path));
                return data == null ? SaveResult.Fail("Save file was empty or invalid.") : SaveResult.Ok();
            }
            catch (Exception ex) { return SaveResult.Fail("Save file is corrupted or unreadable: " + ex.Message); }
        }

        public IReadOnlyList<SaveSlotInfo> EnumerateSlots()
        {
            var slots = new List<SaveSlotInfo>();
            if (!Directory.Exists(_rootPath)) return slots;
            foreach (var file in Directory.GetFiles(_rootPath, "*.json"))
            {
                GameSaveData data = null;
                try { data = _saveGameService.FromJson(File.ReadAllText(file)); } catch { }
                slots.Add(new SaveSlotInfo { SlotId = Path.GetFileNameWithoutExtension(file), Path = file, Metadata = data?.metadata });
            }
            return slots;
        }

        public bool Delete(string slotId)
        {
            var path = GetSlotPath(slotId);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        private static string Sanitize(string slotId)
        {
            var value = string.IsNullOrWhiteSpace(slotId) ? "slot" : slotId;
            foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value;
        }
    }
}
