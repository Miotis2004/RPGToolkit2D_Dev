using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Editor;

namespace SixStringSyn.RPGToolkit2D.Tests.Editor
{
    public sealed class RPGToolkitPackageValidatorTests
    {
        [Test]
        public void FoundationValidationPasses()
        {
            Assert.That(RPGToolkitPackageValidator.IsFoundationValid(), Is.True);
        }

        [Test]
        public void FoundationValidationIncludesPackageMetadata()
        {
            var foundMetadataRule = false;

            foreach (var result in RPGToolkitPackageValidator.ValidatePackageFoundation())
            {
                if (result.RuleName == "Package metadata")
                {
                    foundMetadataRule = true;
                    Assert.That(result.Passed, Is.True, result.Message);
                }
            }

            Assert.That(foundMetadataRule, Is.True);
        }
    }
}
