using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace NuvLoc.Config;

public static class LanguageCode
{
    public const string Hint = "Use a BCP-47 culture such as es, fr, or pt-BR.";
    public const string SkipFile = "Skipped creating a localized .resx for this code.";
    public const string IssuesUrl = "https://github.com/nuvyntralabs/NuvLoc/issues";

    public static bool TryNormalize(
        string? raw,
        [NotNullWhen(true)] out string? normalized,
        [NotNullWhen(false)] out string? error)
    {
        normalized = null;
        error = null;

        if (string.IsNullOrWhiteSpace(raw))
        {
            error = $"Invalid language code: empty or blank is not a BCP-47 culture. {SkipFile}";
            return false;
        }

        var trimmed = raw.Trim();
        if (trimmed.Contains('_'))
        {
            error = $"Invalid language code '{trimmed}': BCP-47 uses a hyphen (example: es-MX), not an underscore. {SkipFile}";
            return false;
        }

        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(trimmed, predefinedOnly: true);
        }
        catch (CultureNotFoundException)
        {
            error = $"Invalid language code '{trimmed}': not a BCP-47 culture. {SkipFile}";
            return false;
        }

        if (culture.Equals(CultureInfo.InvariantCulture) || string.IsNullOrEmpty(culture.Name))
        {
            error = $"Invalid language code '{trimmed}': not a BCP-47 culture. {SkipFile}";
            return false;
        }

        normalized = culture.Name;
        return true;
    }

    public static IReadOnlyList<string> NormalizeAll(
        IEnumerable<string> codes,
        List<ConfigError> errors)
    {
        var normalized = new List<string>();
        foreach (var code in codes)
        {
            if (!TryNormalize(code, out var name, out var error))
            {
                errors.Add(new ConfigError(error));
                continue;
            }

            if (!normalized.Contains(name, StringComparer.OrdinalIgnoreCase))
                normalized.Add(name);
        }

        return normalized;
    }
}
