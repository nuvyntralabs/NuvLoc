using System.Text.RegularExpressions;

namespace NuvLoc.Diff;

public static partial class PlaceholderValidator
{
    public static IReadOnlyList<string> Tokens(string value)
    {
        var tokens = new List<string>();
        foreach (Match match in TokenRegex().Matches(value ?? ""))
        {
            if (match.Value.StartsWith("{{", StringComparison.Ordinal)
                || match.Value.StartsWith("}}", StringComparison.Ordinal))
            {
                continue;
            }

            tokens.Add(match.Value);
        }

        tokens.Sort(StringComparer.Ordinal);
        return tokens;
    }

    public static bool Matches(string source, string target)
    {
        var expected = Tokens(source);
        var actual = Tokens(target);
        if (expected.Count != actual.Count)
            return false;

        for (var i = 0; i < expected.Count; i++)
        {
            if (!string.Equals(expected[i], actual[i], StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    [GeneratedRegex("""\{\{|\}\}|\{[0-9]+\}|\{[A-Za-z_][A-Za-z0-9_]*\}|%[0-9]*\$?[sd]""", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();
}
