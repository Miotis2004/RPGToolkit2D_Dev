using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityPackageInfo = UnityEditor.PackageManager.PackageInfo;

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
                ValidateDirectory("Samples~/BasicRPG"),
                ValidateDirectory("Samples~/MapWorkflow"),
                ValidateDirectory("Samples~/MapWorkflow/Scripts"),
                ValidateDirectory("Documentation~"),
                ValidateFile("README.md"),
                ValidateFile("CHANGELOG.md"),
                ValidateFile("LICENSE.md"),
                ValidateFile("Samples~/BasicRPG/README.md"),
                ValidateFile("Samples~/MapWorkflow/README.md"),
                ValidateFile("Samples~/MapWorkflow/Scripts/MapWorkflowSampleController.cs"),
                ValidateFile("Samples~/MapWorkflow/Scripts/MapTransitionLogger.cs"),
                ValidateFile("Documentation~/index.md"),
                ValidateFile("Documentation~/maps-workflow.md"),
                ValidateFile("Documentation~/api.md"),
                ValidateFile("Documentation~/extension-guide.md"),
                ValidateFile("Documentation~/troubleshooting.md"),
                ValidateFile("Documentation~/upgrade-guide.md"),
                ValidateFile("Runtime/com.sixstringsyn.rpgtoolkit2d.runtime.asmdef"),
                ValidateFile("Editor/com.sixstringsyn.rpgtoolkit2d.editor.asmdef"),
                ValidateFile("Tests/Runtime/com.sixstringsyn.rpgtoolkit2d.tests.runtime.asmdef"),
                ValidateFile("Tests/Editor/com.sixstringsyn.rpgtoolkit2d.tests.editor.asmdef"),
                ValidateRuntimeAssemblyHasNoEditorReferences(),
                ValidatePackageMetaFiles()
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
            var packageInfo = UnityPackageInfo.FindForPackageName(PackageName);
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

        private static PackageValidationResult ValidateRuntimeAssemblyHasNoEditorReferences()
        {
            var runtimePath = Path.Combine(PackagePath, "Runtime");
            if (!Directory.Exists(runtimePath))
            {
                return new PackageValidationResult("Runtime editor reference scan", false, runtimePath);
            }

            foreach (var file in Directory.GetFiles(runtimePath, "*.cs", SearchOption.AllDirectories))
            {
                var contents = File.ReadAllText(file);
                if (contents.Contains("using UnityEditor") || contents.Contains("UnityEditor."))
                {
                    return new PackageValidationResult("Runtime editor reference scan", false, $"Editor-only API reference found in {file}.");
                }
            }

            return new PackageValidationResult("Runtime editor reference scan", true, "Runtime assembly contains no UnityEditor references.");
        }

        private static PackageValidationResult ValidatePackageMetaFiles()
        {
            foreach (var file in Directory.GetFiles(PackagePath, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta") || file.Contains($"{Path.DirectorySeparatorChar}Samples~{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                var metaPath = file + ".meta";
                if (!File.Exists(metaPath))
                {
                    return new PackageValidationResult("Package meta file scan", false, $"Missing meta file for {file}.");
                }
            }

            return new PackageValidationResult("Package meta file scan", true, "All package files outside Samples~ have .meta files.");
        }
    }
}
