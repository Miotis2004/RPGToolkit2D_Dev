using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public static class DialogueConditionEvaluator
    {
        public static bool AreMet(IEnumerable<DialogueCondition> conditions, IDialogueContext context)
        {
            return conditions == null || conditions.All(condition => IsMet(condition, context));
        }

        public static bool IsMet(DialogueCondition condition, IDialogueContext context)
        {
            if (condition == null || string.IsNullOrWhiteSpace(condition.Key)) return true;
            var exists = context != null && context.TryGetValue(condition.Key, out var actual);
            switch (condition.Operator)
            {
                case DialogueConditionOperator.Exists: return exists;
                case DialogueConditionOperator.NotExists: return !exists;
                case DialogueConditionOperator.Equals: return exists && string.Equals(actual, condition.Value, StringComparison.OrdinalIgnoreCase);
                case DialogueConditionOperator.NotEquals: return !exists || !string.Equals(actual, condition.Value, StringComparison.OrdinalIgnoreCase);
                case DialogueConditionOperator.GreaterThan: return Compare(actual, condition.Value) > 0;
                case DialogueConditionOperator.GreaterThanOrEqual: return Compare(actual, condition.Value) >= 0;
                case DialogueConditionOperator.LessThan: return Compare(actual, condition.Value) < 0;
                case DialogueConditionOperator.LessThanOrEqual: return Compare(actual, condition.Value) <= 0;
                default: return false;
            }
        }

        private static int Compare(string actual, string expected)
        {
            if (double.TryParse(actual, NumberStyles.Float, CultureInfo.InvariantCulture, out var left) && double.TryParse(expected, NumberStyles.Float, CultureInfo.InvariantCulture, out var right))
            {
                return left.CompareTo(right);
            }
            return string.Compare(actual, expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
