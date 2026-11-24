namespace MangaLegends.Core.Content
{
    public enum ValidationSeverity
    {
        Warning,
        Error
    }

    public readonly struct ValidationIssue
    {
        public ValidationIssue(ValidationSeverity severity, string contentId, string message)
        {
            Severity = severity;
            ContentId = contentId;
            Message = message;
        }

        public ValidationSeverity Severity { get; }
        public string ContentId { get; }
        public string Message { get; }

        public override string ToString() => $"[{Severity}] {ContentId}: {Message}";
    }
}
