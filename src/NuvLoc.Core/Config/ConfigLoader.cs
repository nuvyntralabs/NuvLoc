using System.Text.Json;

namespace NuvLoc.Config;

public static class ConfigLoader
{
    static readonly HashSet<string> SourceCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "en-US", "en-GB",
    };

    static readonly HashSet<string> KnownPlatforms = new(StringComparer.OrdinalIgnoreCase)
    {
        "maui",
        "wpf",
        "winui",
        "avalonia",
        "uno",
    };

    const string SupportedPlatforms = "maui, wpf, winui, avalonia, uno";

    public static ConfigLoadResult Load(string configPath)
    {
        var full = Path.GetFullPath(configPath);
        if (!File.Exists(full))
        {
            return ConfigLoadResult.Fail(
                $"Config file not found: {full}. Create {I18nConfig.DefaultFileName} with platform, source, and languages.");
        }

        I18nJsonFile? raw;
        try
        {
            raw = JsonSerializer.Deserialize<I18nJsonFile>(
                File.ReadAllText(full),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            return ConfigLoadResult.Fail($"Invalid JSON in {full}: {ex.Message}");
        }

        if (raw is null)
            return ConfigLoadResult.Fail($"Invalid JSON in {full}.");

        var errors = new List<ConfigError>();
        var platform = string.IsNullOrWhiteSpace(raw.Platform) ? null : raw.Platform.Trim();
        var source = string.IsNullOrWhiteSpace(raw.Source) ? null : raw.Source.Trim();
        var languages = raw.Languages?
            .Where(static l => !string.IsNullOrWhiteSpace(l))
            .Select(static l => l.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        if (platform is null)
            errors.Add(new ConfigError("'platform' is required."));
        else if (!KnownPlatforms.Contains(platform))
            errors.Add(new ConfigError($"Unknown platform '{platform}'. Supported: {SupportedPlatforms}."));

        if (source is null)
            errors.Add(new ConfigError("'source' is required."));

        if (languages.Length == 0)
            errors.Add(new ConfigError("'languages' must be a non-empty array of culture codes."));

        var sourceCultures = languages.Where(SourceCultures.Contains).ToArray();
        if (sourceCultures.Length > 0)
            errors.Add(new ConfigError($"Do not list the English source culture in 'languages' ({string.Join(", ", sourceCultures)})."));

        if (errors.Count > 0)
            return new ConfigLoadResult(null, errors);

        var config = new I18nConfig
        {
            Platform = platform!,
            Source = source!,
            Languages = languages,
            ConfigPath = full,
        };

        if (!File.Exists(config.SourceFullPath))
            return ConfigLoadResult.Fail($"Source file not found: {config.SourceFullPath} (relative to {full}).");

        var ext = Path.GetExtension(config.SourceFullPath);
        if (!ext.Equals(".resx", StringComparison.OrdinalIgnoreCase))
            return ConfigLoadResult.Fail($"Supports sibling .resx only. Source is '{ext}'.");

        return new ConfigLoadResult(config, []);
    }

    public static string StubExample() =>
        """
        {
          "platform": "maui",
          "source": "Resources/Strings/AppResources.resx",
          "languages": ["es", "fr"]
        }
        """;

    sealed class I18nJsonFile
    {
        public string? Platform { get; set; }
        public string? Source { get; set; }
        public string[]? Languages { get; set; }
    }
}

public sealed record ConfigLoadResult(I18nConfig? Config, IReadOnlyList<ConfigError> Errors)
{
    public bool Success => Config is not null && Errors.Count == 0;

    public static ConfigLoadResult Fail(string message) =>
        new(null, [new ConfigError(message)]);
}
