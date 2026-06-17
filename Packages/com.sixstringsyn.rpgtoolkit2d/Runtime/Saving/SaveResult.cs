namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    public readonly struct SaveResult
    {
        private SaveResult(bool success, string message)
        {
            Success = success;
            Message = message ?? string.Empty;
        }

        public bool Success { get; }
        public string Message { get; }
        public static SaveResult Ok(string message = "") => new SaveResult(true, message);
        public static SaveResult Fail(string message) => new SaveResult(false, message);
    }
}
