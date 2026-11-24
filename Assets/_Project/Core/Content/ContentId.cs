using System.Text.RegularExpressions;

namespace MangaLegends.Core.Content
{
    public static class ContentId
    {
        private static readonly Regex Pattern = new Regex("^[a-z][a-z0-9_]*(\\.[a-z0-9_]+)+$", RegexOptions.CultureInvariant);

        public static bool IsValid(string id) => !string.IsNullOrEmpty(id) && Pattern.IsMatch(id);

        public static string Domain(string id)
        {
            int dot = id?.IndexOf('.') ?? -1;
            return dot > 0 ? id.Substring(0, dot) : string.Empty;
        }
    }
}
