using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Data;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.State;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Phase1
{
    public sealed class CoreBackboneTests
    {
        [Test]
        public void StateStoreSupportsGlobalsLocalsConditionsAndChangeNotifications()
        {
            var store = new RPGStateStore();
            var changed = new List<string>();
            store.Changed += (key, local, value) => changed.Add($"{(local ? "local" : "global")}:{key}={value}");

            store.SetSwitch("HasMetKing", true);
            store.SetInt("PlayerReputation", 12);
            store.SetString("BridgeRepaired", "yes", local: true);

            Assert.IsTrue(store.GetSwitch("HasMetKing"));
            Assert.AreEqual(12, store.GetInt("PlayerReputation"));
            Assert.AreEqual("yes", store.GetString("BridgeRepaired", local: true));
            Assert.IsTrue(store.Evaluate("PlayerReputation", RPGStateComparison.GreaterOrEqual, "10"));
            Assert.IsTrue(new RPGStateCondition { key = "HasMetKing", comparison = RPGStateComparison.IsTrue }.Evaluate(store));
            Assert.Contains("global:HasMetKing=true", changed);
            Assert.Contains("local:BridgeRepaired=yes", changed);
        }

        [Test]
        public void DatabaseSearchSortsAndDetectsBrokenReferences()
        {
            var potion = ScriptableObject.CreateInstance<ItemDefinition>();
            potion.name = "PotionAsset";
            potion.SetId(new RPGId("item.potion"));
            var database = ScriptableObject.CreateInstance<RPGDatabaseAsset>();
            database.SetRecords(new[] { potion });

            var results = database.Search("potion", sortMode: RPGDatabaseSortMode.Id);

            Assert.AreEqual(potion, results.Single());
            Assert.AreEqual(potion, database.Resolve(new RPGDatabaseReference(RPGContentKind.Item, new RPGId("item.potion"))));
            var validation = database.ValidateReferences(new[] { new RPGDatabaseReference(RPGContentKind.Item, new RPGId("item.missing")) });
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPGDB_BROKEN_REFERENCE"));
        }

        [Test]
        public void SpriteSheetValidationReportsMissingPipelineMetadata()
        {
            var sheet = ScriptableObject.CreateInstance<RPGSpriteSheetAsset>();
            sheet.name = "MonsterSheet";

            var validation = sheet.ValidateSpriteSheet();

            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_SPRITESHEET_MISSING_TEXTURE"));
            Assert.IsTrue(validation.Messages.Any(message => message.Code == "RPG_SPRITESHEET_MISSING_PROFILE"));
        }
    }
}
