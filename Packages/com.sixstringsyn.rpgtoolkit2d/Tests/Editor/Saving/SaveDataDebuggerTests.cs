using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Saving;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor.Saving
{
    public sealed class SaveDataDebuggerTests
    {
        [Test]
        public void SaveDebuggerSupportEnumeratesEmptySlots()
        {
            var service = new SaveSlotService(new SaveGameService(), System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString("N")));
            Assert.That(service.EnumerateSlots(), Is.Empty);
        }
    }
}
