namespace SixStringSyn.RPGToolkit2D.Editor
{
    public readonly struct PackageValidationResult
    {
        public PackageValidationResult(string ruleName, bool passed, string message)
        {
            RuleName = ruleName;
            Passed = passed;
            Message = message;
        }

        public string RuleName { get; }

        public bool Passed { get; }

        public string Message { get; }
    }
}
