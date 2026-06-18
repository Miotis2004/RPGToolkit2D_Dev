using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;

namespace SixStringSyn.RPGToolkit2D.Editor
{
    public static class RPGToolkitPackageValidator
    {
        public const string PackageName = "com.sixstringsyn.rpgtoolkit2d";
        public const string DisplayName = "RPG Toolkit 2D";
        public const string Version = "1.0.0";
        public const string PackagePath = "Packages/" + PackageName;

        public static IReadOnlyList<PackageValidationResult> ValidatePackageFoundation()
        {
            var results = new List<PackageValidationResult>
            {
                ValidatePackageInfo(),
                ValidateDirectory("Runtime"),
                ValidateDirectory("Editor"),
                ValidateDirectory("Tests"),
                ValidateDirectory("Samples~"),
                ValidateDirectory("Documentation~"),
                ValidateFile("README.md"),
                ValidateFile("CHANGELOG.md"),
                ValidateFile("LICENSE.md"),
                ValidateFile("Documentation~/index.md"),
                ValidateFile("Documentation~/api.md"),
                ValidateFile("Documentation~/extension-guide.md"),
                ValidateFile("Documentation~/troubleshooting.md"),
                ValidateFile("Documentation~/upgrade-guide.md"),
                ValidateFile("Runtime/com.sixstringsyn.rpgtoolkit2d.runtime.asmdef"),
                ValidateFile("Editor/com.sixstringsyn.rpgtoolkit2d.editor.asmdef"),
                ValidateFile("Tests/Runtime/com.sixstringsyn.rpgtoolkit2d.tests.runtime.asmdef"),
                ValidateFile("Tests/Editor/com.sixstringsyn.rpgtoolkit2d.tests.editor.asmdef")
            };

            return results;
        }

        public static bool IsFoundationValid()
        {
            foreach (var result in ValidatePackageFoundation())
            {
                if (!result.Passed)
                {
                    return false;
                }
            }

            return true;
        }

        private static PackageValidationResult ValidatePackageInfo()
        {
            var packageInfo = PackageInfo.FindForPackageName(PackageName);
            if (packageInfo == null)
            {
                return new PackageValidationResult("Package metadata", false, $"Package '{PackageName}' was not found by Package Manager.");
            }

            var passed = packageInfo.displayName == DisplayName && packageInfo.version == Version;
            var message = passed
                ? $"Found {packageInfo.displayName} {packageInfo.version}."
                : $"Expected {DisplayName} {Version}, found {packageInfo.displayName} {packageInfo.version}.";

            return new PackageValidationResult("Package metadata", passed, message);
        }

        private static PackageValidationResult ValidateDirectory(string relativePath)
        {
            var path = Path.Combine(PackagePath, relativePath);
            return new PackageValidationResult($"Directory {relativePath}", Directory.Exists(path), path);
        }

        private static PackageValidationResult ValidateFile(string relativePath)
        {
            var path = Path.Combine(PackagePath, relativePath);
            return new PackageValidationResult($"File {relativePath}", File.Exists(path), path);
        }
    }
}
