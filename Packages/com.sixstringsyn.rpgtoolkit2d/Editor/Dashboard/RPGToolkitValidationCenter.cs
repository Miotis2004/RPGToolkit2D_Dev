using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Dashboard
{
    public sealed class RPGToolkitValidationDiagnostic
    {
        public RPGToolkitValidationDiagnostic(RPGToolkitAuthoringSection section, UnityEngine.Object asset, string assetPath, RPGValidationMessage message, string repairPreview = null, Action repairAction = null)
        {
            Section = section ?? throw new ArgumentNullException(nameof(section));
            Asset = asset;
            AssetPath = assetPath ?? string.Empty;
            Message = message;
            RepairPreview = repairPreview ?? string.Empty;
            RepairAction = repairAction;
        }

        public RPGToolkitAuthoringSection Section { get; }
        public UnityEngine.Object Asset { get; }
        public string AssetPath { get; }
        public RPGValidationMessage Message { get; }
        public string RepairPreview { get; }
        public Action RepairAction { get; }
        public bool HasRepair => RepairAction != null;
    }

    public sealed class RPGToolkitValidationReport
    {
        private readonly List<RPGToolkitValidationDiagnostic> _diagnostics = new List<RPGToolkitValidationDiagnostic>();

        public IReadOnlyList<RPGToolkitValidationDiagnostic> Diagnostics => _diagnostics;
        public int ErrorCount => _diagnostics.Count(diagnostic => diagnostic.Message.Severity == RPGValidationSeverity.Error);
        public int WarningCount => _diagnostics.Count(diagnostic => diagnostic.Message.Severity == RPGValidationSeverity.Warning);
        public int InfoCount => _diagnostics.Count(diagnostic => diagnostic.Message.Severity == RPGValidationSeverity.Info);
        public bool Passed => ErrorCount == 0;

        public void Add(RPGToolkitValidationDiagnostic diagnostic)
        {
            if (diagnostic != null) _diagnostics.Add(diagnostic);
        }
    }

    public static class RPGToolkitValidationCenter
    {
        public static RPGToolkitValidationReport ValidateAllRPGContent()
        {
            var report = new RPGToolkitValidationReport();
            foreach (var section in RPGToolkitAuthoringWorkflow.Sections) ValidateSection(section, report);
            return report;
        }

        public static RPGToolkitValidationReport ValidateSection(RPGToolkitAuthoringSection section)
        {
            var report = new RPGToolkitValidationReport();
            ValidateSection(section, report);
            return report;
        }

        public static IReadOnlyList<RPGToolkitValidationDiagnostic> Filter(
            RPGToolkitValidationReport report,
            bool includeErrors,
            bool includeWarnings,
            bool includeInfo,
            string searchText)
        {
            if (report == null) return Array.Empty<RPGToolkitValidationDiagnostic>();
            return report.Diagnostics.Where(diagnostic => MatchesSeverity(diagnostic.Message.Severity, includeErrors, includeWarnings, includeInfo) && MatchesSearch(diagnostic, searchText)).ToList();
        }

        public static string ExportMarkdown(RPGToolkitValidationReport report)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# RPG Toolkit Validation Report");
            builder.AppendLine();
            if (report == null)
            {
                builder.AppendLine("No validation report has been generated yet.");
                return builder.ToString();
            }

            builder.AppendLine($"Summary: {report.ErrorCount} error(s), {report.WarningCount} warning(s), {report.InfoCount} info message(s).");
            builder.AppendLine();
            foreach (var group in report.Diagnostics.GroupBy(diagnostic => diagnostic.Section.Title).OrderBy(group => group.Key))
            {
                builder.AppendLine($"## {group.Key}");
                foreach (var diagnostic in group.OrderByDescending(diagnostic => diagnostic.Message.Severity).ThenBy(diagnostic => diagnostic.AssetPath))
                {
                    builder.AppendLine($"- **{diagnostic.Message.Severity}** `{diagnostic.Message.Code}` {diagnostic.Message.Message} ({diagnostic.AssetPath})");
                    if (diagnostic.HasRepair) builder.AppendLine($"  - Repair preview: {diagnostic.RepairPreview}");
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public static int RepairAllSafe(RPGToolkitValidationReport report)
        {
            if (report == null) return 0;
            var repaired = 0;
            foreach (var diagnostic in report.Diagnostics.Where(diagnostic => diagnostic.HasRepair))
            {
                diagnostic.RepairAction.Invoke();
                repaired++;
            }
            if (repaired > 0) AssetDatabase.SaveAssets();
            return repaired;
        }

        private static void ValidateSection(RPGToolkitAuthoringSection section, RPGToolkitValidationReport report)
        {
            foreach (var entry in RPGToolkitAuthoringWorkflow.FindAssets(section))
            {
                var result = RPGToolkitAuthoringWorkflow.ValidateAsset(entry.Asset);
                if (result != null)
                {
                    foreach (var message in result.Messages) report.Add(CreateDiagnostic(section, entry, message));
                }

                if (entry.DuplicateId)
                {
                    report.Add(CreateDuplicateIdDiagnostic(section, entry));
                }
            }
        }

        private static RPGToolkitValidationDiagnostic CreateDiagnostic(RPGToolkitAuthoringSection section, RPGToolkitAssetBrowserEntry entry, RPGValidationMessage message)
        {
            return new RPGToolkitValidationDiagnostic(section, entry.Asset, entry.Path, message);
        }

        private static RPGToolkitValidationDiagnostic CreateDuplicateIdDiagnostic(RPGToolkitAuthoringSection section, RPGToolkitAssetBrowserEntry entry)
        {
            var message = new RPGValidationMessage(RPGValidationSeverity.Error, "RPG_DASHBOARD_DUPLICATE_ID", $"{entry.Asset.name} shares an RPG ID with another {section.Title} asset.");
            if (!(entry.Asset is RPGObject rpgObject)) return new RPGToolkitValidationDiagnostic(section, entry.Asset, entry.Path, message);
            return new RPGToolkitValidationDiagnostic(section, entry.Asset, entry.Path, message, $"Assign a new generated RPG ID to {entry.Asset.name}.", () =>
            {
                Undo.RecordObject(rpgObject, "Repair Duplicate RPG ID");
                rpgObject.AssignNewId();
                EditorUtility.SetDirty(rpgObject);
            });
        }

        private static bool MatchesSeverity(RPGValidationSeverity severity, bool includeErrors, bool includeWarnings, bool includeInfo)
        {
            return (severity == RPGValidationSeverity.Error && includeErrors) || (severity == RPGValidationSeverity.Warning && includeWarnings) || (severity == RPGValidationSeverity.Info && includeInfo);
        }

        private static bool MatchesSearch(RPGToolkitValidationDiagnostic diagnostic, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return true;
            return diagnostic.Section.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                || diagnostic.AssetPath.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                || diagnostic.Message.Code.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                || diagnostic.Message.Message.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                || (diagnostic.Asset != null && diagnostic.Asset.name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
