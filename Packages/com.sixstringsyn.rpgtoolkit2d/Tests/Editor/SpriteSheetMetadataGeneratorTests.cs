using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor.Windows;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor
{
    public sealed class SpriteSheetMetadataGeneratorTests
    {
        [Test]
        public void GridGeneratorCreatesDeterministicFrameRectangles()
        {
            var texture = new Texture2D(32, 16);
            var profile = ScriptableObject.CreateInstance<RPGSpriteSheetProfile>();

            var frames = RPGSpriteSheetMetadataGenerator.GenerateGridFrames(texture, profile, "Test Sheet");

            Assert.AreEqual(2, frames.Count);
            Assert.AreEqual("test_sheet_0_0", frames[0].frameId);
            Assert.AreEqual(new RectInt(0, 0, 16, 16), frames[0].sourceRect);
            Assert.AreEqual(new RectInt(16, 0, 16, 16), frames[1].sourceRect);
        }
    }
}
