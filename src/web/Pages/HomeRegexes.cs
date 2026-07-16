using System.Text.RegularExpressions;

namespace web.Pages;

internal static partial class HomeRegexes
{
    [GeneratedRegex("(?im)^(title|heading)\\s*[:\\-]\\s*(.+)$")]
    public static partial Regex TitleRegex();

    [GeneratedRegex("(?im)^(summary|overview)\\s*[:\\-]\\s*(.+)$")]
    public static partial Regex SummaryRegex();

    [GeneratedRegex("\\s+")]
    public static partial Regex WhitespaceRegex();

    [GeneratedRegex("(?<=[.!?])\\s+")]
    public static partial Regex SentenceBoundaryRegex();
}
