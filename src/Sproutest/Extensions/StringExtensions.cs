using System.Text.RegularExpressions;

namespace Sproutest.Extensions;

internal static class StringExtensions
{
    private static readonly Regex RegexJsonPattern = new("(?<json>{(?:[^{}]|(?<Nested>{)|(?<-Nested>}))*(?(Nested)(?!))})", RegexOptions.Compiled);

    public static bool IsValidJson(this string content)
    {
        return RegexJsonPattern.IsMatch(content);
    }
}