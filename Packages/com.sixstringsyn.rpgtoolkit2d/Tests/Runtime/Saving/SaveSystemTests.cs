using System.IO;
using NUnit.Framework;
using System.Reflection;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Saving;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Saving
{
    public sealed class SaveSystemTests
    {
        [Test]
        public void JsonSerializationRoundTripsInventoryContributor()
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>(); item.SetId(new RPGId("item.potion"));
            item.GetType().GetField("_maximumStackSize", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(item, 10);
            InventoryContainer inventory = new InventoryContainer(4); inventory.Add(item, 2);
            var service = new SaveGameService();
            service.Register(new InventorySaveContributor(() => inventory, value => inventory = value, id => id == item.Id ? item : null, 4));
            var json = service.ToJson(service.Capture(10d, "TestScene"));
            inventory = new InventoryContainer(4);
            Assert.That(service.Restore(service.FromJson(json)).Success, Is.True);
            Assert.That(inventory.Count(item), Is.EqualTo(2));
            Object.DestroyImmediate(item);
        }

        [Test]
        public void SaveSlotServiceHandlesMissingAndCorruptedFilesGracefully()
        {
            var root = Path.Combine(Path.GetTempPath(), "RPGToolkit2D_SaveTests", System.Guid.NewGuid().ToString("N"));
            var slots = new SaveSlotService(new SaveGameService(), root);
            Assert.That(slots.Load("missing", out _).Success, Is.False);
            Directory.CreateDirectory(root); File.WriteAllText(slots.GetSlotPath("bad"), "{ not json");
            Assert.That(slots.Load("bad", out _).Success, Is.False);
            Directory.Delete(root, true);
        }

        [Test]
        public void VersionMismatchFailsWithoutMigration()
        {
            var service = new SaveGameService();
            var data = new GameSaveData(); data.metadata.saveVersion = "999.0.0";
            Assert.That(service.Restore(data).Success, Is.False);
        }
    }
}
