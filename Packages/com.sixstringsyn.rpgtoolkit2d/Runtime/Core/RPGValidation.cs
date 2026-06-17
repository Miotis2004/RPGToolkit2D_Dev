using System.Collections.Generic;
using System.Linq;

namespace SixStringSyn.RPGToolkit2D.Runtime.Core
{
    public enum RPGValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public readonly struct RPGValidationMessage
    {
        public RPGValidationMessage(RPGValidationSeverity severity, string code, string message, RPGId relatedId = default)
        {
            Severity = severity;
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            RelatedId = relatedId;
        }

        public RPGValidationSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public RPGId RelatedId { get; }
        public bool IsError => Severity == RPGValidationSeverity.Error;
    }

    public sealed class RPGValidationResult
    {
        private readonly List<RPGValidationMessage> _messages = new List<RPGValidationMessage>();

        public IReadOnlyList<RPGValidationMessage> Messages => _messages;
        public bool IsValid => !_messages.Any(message => message.IsError);

        public void Add(RPGValidationSeverity severity, string code, string message, RPGId relatedId = default)
        {
            _messages.Add(new RPGValidationMessage(severity, code, message, relatedId));
        }

        public void AddError(string code, string message, RPGId relatedId = default) => Add(RPGValidationSeverity.Error, code, message, relatedId);
        public void AddWarning(string code, string message, RPGId relatedId = default) => Add(RPGValidationSeverity.Warning, code, message, relatedId);
        public void AddInfo(string code, string message, RPGId relatedId = default) => Add(RPGValidationSeverity.Info, code, message, relatedId);
    }
}
