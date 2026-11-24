using System.Collections.Generic;

namespace MangaLegends.Core.Content
{
    public sealed class ValidationReport
    {
        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();

        public IReadOnlyList<ValidationIssue> Issues => _issues;

        public bool HasErrors
        {
            get
            {
                foreach (ValidationIssue issue in _issues)
                    if (issue.Severity == ValidationSeverity.Error) return true;
                return false;
            }
        }

        public void Error(string contentId, string message) =>
            _issues.Add(new ValidationIssue(ValidationSeverity.Error, contentId, message));

        public void Warning(string contentId, string message) =>
            _issues.Add(new ValidationIssue(ValidationSeverity.Warning, contentId, message));

        public void CheckIds(IEnumerable<IContentEntry> entries)
        {
            HashSet<string> seen = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (IContentEntry entry in entries)
            {
                if (entry == null) continue;
                if (!ContentId.IsValid(entry.Id)) Error(entry.Id ?? "<null>", "Invalid id: expected 'domain.name' in lowercase snake_case.");
                else if (!seen.Add(entry.Id)) Error(entry.Id, "Duplicate id.");
            }
        }

        public void CheckRankArray(string contentId, string field, int length, int expected)
        {
            if (length == expected || length == 1) return;
            Error(contentId, $"{field} has {length} values but the ability has {expected} ranks (use 1 value for a constant).");
        }
    }
}
